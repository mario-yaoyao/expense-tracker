using ExpenseTracker.Data;
using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseTracker.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<(TokenResDto? Token, string? Error)> LoginAsync(LoginUserReqDto request)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user is null) return (null, "user_not_found");
            if (!user.IsActive) return (null, "account_inactive");
            if (!IsPasswordValid(user, request.Password)) return (null, "invalid_password");

            return (await CreateTokenResponse(user), null);
        }

        public async Task<(User? User, string? Error)> RegisterAsync(RegisterReqDto request)
        {
            if (await IsUsernameTaken(request.Username)) return (null, "duplicate_name");

            var user = new User();
            user.FullName = request.FullName;
            user.Username = request.Username;
            user.ContactNumber = request.ContactNumber;
            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return (user, null);
        }

        private static bool IsPasswordValid(User user, string password) =>
            new PasswordHasher<User>().VerifyHashedPassword(user, user.HashedPassword, password) != PasswordVerificationResult.Failed;

        private async Task<bool> IsUsernameTaken(string username) =>
            await context.Users.AnyAsync(u => u.Username == username);

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

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

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
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
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
