using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Common;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ExpenseTracker.BLL.Services
{
    public class AuthService(IConfiguration configuration, IAuthRepository authRepository, IEmailService emailService) : IAuthService
    {
        public async Task<ServiceResult<TokenResDto>> LoginAsync(EncryptedReqDto request)
        {
            var json = Decrypt(request.EncryptedData);
            var loginRequest = JsonSerializer.Deserialize<LoginReqDto>(json);

            var user = await authRepository.GetByUsernameAsync(loginRequest!.Username);

            if (user == null)
            {
                return new ServiceResult<TokenResDto>
                {
                    Success = false,
                    ErrorMessage = "No account found with that username."
                };
            }

            if (!user.IsActive)
            {
                return new ServiceResult<TokenResDto>
                {
                    Success = false,
                    ErrorMessage = "Your account has been deactivated."
                };
            }

            if (!IsPasswordValid(user, loginRequest.Password))
            {
                return new ServiceResult<TokenResDto>
                {
                    Success = false,
                    ErrorMessage = "Incorrect password."
                };
            }

            Log.ForContext("UserId", user.Id)
               .ForContext("Username", user.Username)
               .ForContext("Action", "Info")
               .ForContext("EntityName", "Auth")
               .ForContext("Activity", "Account logged in.")
               .Information($"'{user.Username}' account logged in.");

            return new ServiceResult<TokenResDto>
            {
                Success = true,
                Data = await CreateTokenResponse(user)
            };
        }

        public async Task<ServiceResult<object>> RegisterAsync(EncryptedReqDto request)
        {
            var json = Decrypt(request.EncryptedData);
            var registerRequest = JsonSerializer.Deserialize<RegisterReqDto>(json);

            if (registerRequest!.Password != registerRequest.ConfirmPassword)
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "Passwords do not match."
                };
            }

            if (await IsUsernameTaken(registerRequest.Username))
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "Username is already taken."
                };
            }

            var user = new User
            {
                FullName = registerRequest.FullName,
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                ContactNumber = registerRequest.ContactNumber,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, registerRequest.Password);

            await authRepository.AddUserAsync(user);

            Log.ForContext("UserId", user.Id)
               .ForContext("Username", user.Username)
               .ForContext("Action", "Create")
               .ForContext("EntityName", "Auth")
               .ForContext("Activity", "Account registered.")
               .Information($"'{user.Username}' account registered.");

            return new ServiceResult<object>
            {
                Success = true
            };
        }

        public async Task<bool> ForgotPasswordAsync(EncryptedReqDto request)
        {
            var json = Decrypt(request.EncryptedData);
            var forgotPasswordRequest = JsonSerializer.Deserialize<ForgotPasswordReqDto>(json);

            var user = await authRepository.GetByEmailAsync(forgotPasswordRequest!.Email);

            if (user == null) return false;

            var resetToken = await GenerateAndSaveResetTokenAsync(user);

            var emailSent = await emailService.SendEmailAsync(
                user.FullName,
                user.Username,
                user.Email,
                "Reset your BudgetWise password",
                $"""
                We received a request to reset your BudgetWise password. Use the button below to create your new password.
                """,
                resetToken);

            return emailSent;
        }

        public async Task<ServiceResult<object>> ResetPasswordAsync(EncryptedReqDto request)
        {
            var json = Decrypt(request.EncryptedData);
            var resetPasswordRequest = JsonSerializer.Deserialize<ResetPasswordReqDto>(json);

            if (resetPasswordRequest!.NewPassword != resetPasswordRequest.ConfirmNewPassword)
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "Passwords do not match."
                };
            }

            var user = await authRepository.GetUserByResetToken(resetPasswordRequest.Token);

            if (user == null)
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "Invalid reset token."
                };
            }

            if (user.ResetTokenExpiryTime < DateTime.UtcNow)
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "Reset token has expired. Please submit a new password reset request."
                };
            }

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, resetPasswordRequest.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            await authRepository.UpdatePasswordAsync(user);

            Log.ForContext("UserId", user.Id)
               .ForContext("Username", user.Username)
               .ForContext("Action", "Update")
               .ForContext("EntityName", "Auth")
               .ForContext("Activity", "Password reset'.")
               .Information($"'{user.Username}' reset their password.");

            return new ServiceResult<object>
            {
                Success = true
            };
        }

        private static bool IsPasswordValid(User user, string password) =>
            new PasswordHasher<User>().VerifyHashedPassword(user, user.HashedPassword, password) != PasswordVerificationResult.Failed;

        private async Task<bool> IsUsernameTaken(string username) =>
            await authRepository.IsUsernameTakenAsync(username);

        private async Task<TokenResDto> CreateTokenResponse(User user)
        {
            return new TokenResDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<TokenResDto?> RefreshTokensAsync(EncryptedReqDto request)
        {
            var json = Decrypt(request.EncryptedData);
            var refreshRequest = JsonSerializer.Deserialize<RefreshTokenReqDto>(json);

            var user = await ValidateRefreshTokenAsync(refreshRequest!.UserId, refreshRequest.RefreshToken);

            if (user is null) return null;

            return await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await authRepository.GetByIdAsync(userId);

            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow) return null;

            return user;
        }

        private static string GenerateRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomNumber = new byte[32];
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRandomToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await authRepository.SaveChangesAsync();

            return refreshToken;
        }

        private async Task<string> GenerateAndSaveResetTokenAsync(User user)
        {
            var resetToken = GenerateRandomToken();
            user.ResetToken = resetToken;
            user.ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(5);
            await authRepository.SaveChangesAsync();

            return resetToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public static string Decrypt(string encryptedData)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedData);

            using var rsa = RSA.Create();

            rsa.ImportFromPem(System.IO.File.ReadAllText("Keys/private.pem"));

            var decryptedBytes = rsa.Decrypt(
                encryptedBytes,
                RSAEncryptionPadding.OaepSHA256);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
