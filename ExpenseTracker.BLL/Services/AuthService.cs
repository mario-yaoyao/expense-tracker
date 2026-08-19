using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using ExpenseTracker.Models.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseTracker.BLL.Services
{
    public class AuthService(IConfiguration configuration, IAuthRepository authRepository) : IAuthService
    {
        public async Task<ServiceResult<TokenResDto>> LoginAsync(LoginUserReqDto request)
        {
            var user = await authRepository.GetByUsernameAsync(request.Username);

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

            if (!IsPasswordValid(user, request.Password))
            {
                return new ServiceResult<TokenResDto>
                {
                    Success = false,
                    ErrorMessage = "Incorrect password."
                };
            }

            return new ServiceResult<TokenResDto>
            {
                Success = true,
                Data = await CreateTokenResponse(user)
            };
        }

        public async Task<ServiceResult<RegisterResDto>> RegisterAsync(RegisterReqDto request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return new ServiceResult<RegisterResDto>
                {
                    Success = false,
                    ErrorMessage = "Passwords do not match."
                };
            }

            if (await IsUsernameTaken(request.Username))
            {
                return new ServiceResult<RegisterResDto>
                {
                    Success = false,
                    ErrorMessage = "Username is already taken."
                };
            }

            var user = new User
            {
                FullName = request.FullName,
                Username = request.Username,
                Email = request.Email,
                ContactNumber = request.ContactNumber,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            await authRepository.AddUserAsync(user);

            return new ServiceResult<RegisterResDto>
            {
                Success = true,
                Data = new RegisterResDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Username = user.Username,
                    Email = user.Email,
                    ContactNumber = user.ContactNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
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

        public async Task<TokenResDto?> RefreshTokensAsync(RefreshTokenReqDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);

            if (user is null) return null;

            return await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await authRepository.GetByIdAsync(userId);

            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow) return null;

            return user;
        }

        private static string GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomNumber = new byte[32];
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await authRepository.SaveChangesAsync();

            return refreshToken;
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
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
