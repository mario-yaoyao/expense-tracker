using ExpenseTracker.Data;
using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegisterAsync_ReturnsUser_WhenRegistrationSucceeds()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var authService = new AuthService(context, configuration);

            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Password = "Password123!"
            };

            // Act
            var (user, error) = await authService.RegisterAsync(request);

            // Assert
            Assert.Null(error);
            Assert.NotNull(user);

            Assert.Equal(request.FullName, user.FullName);
            Assert.Equal(request.Username, user.Username);
            Assert.Equal(request.ContactNumber, user.ContactNumber);

            Assert.NotEqual(request.Password, user.HashedPassword);
            Assert.True(user.IsActive);

            var savedUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            Assert.NotNull(savedUser);
            Assert.Equal(user.Id, savedUser.Id);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsDuplicateName_WhenUsernameAlreadyExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var existingUser = new User
            {
                FullName = "Existing User",
                Username = "testuser",
                ContactNumber = "09123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            existingUser.HashedPassword = new PasswordHasher<User>()
                .HashPassword(existingUser, "Password123!");

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration);

            var request = new RegisterReqDto
            {
                FullName = "New User",
                Username = "testuser", // same username
                ContactNumber = "09876543210",
                Password = "Password123!"
            };

            // Act
            var (user, error) = await authService.RegisterAsync(request);

            // Assert
            Assert.Null(user);
            Assert.Equal("duplicate_name", error);

            var userCount = await context.Users.CountAsync();

            Assert.Equal(1, userCount);
        }

        [Fact]
        public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var user = new User
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.HashedPassword = new PasswordHasher<User>()
                .HashPassword(user, "Password123!");

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration);

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var (token, error) = await authService.LoginAsync(request);

            // Assert
            Assert.Null(error);

            Assert.NotNull(token);
            Assert.NotNull(token.AccessToken);
            Assert.NotNull(token.RefreshToken);

            var savedUser = await context.Users
                .FirstAsync(u => u.Id == user.Id);

            Assert.Equal(token.RefreshToken, savedUser.RefreshToken);
            Assert.True(savedUser.RefreshTokenExpiryTime > DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUserNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var authService = new AuthService(context, configuration);

            var request = new LoginUserReqDto
            {
                Username = "unknownuser",
                Password = "Password123!"
            };

            // Act
            var (token, error) = await authService.LoginAsync(request);

            // Assert
            Assert.Null(token);
            Assert.Equal("user_not_found", error);
        }

        [Fact]
        public async Task LoginAsync_ReturnsAccountInactive_WhenUserIsInactive()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var existingUser = new User
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09123456789",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            existingUser.HashedPassword = new PasswordHasher<User>()
                .HashPassword(existingUser, "Password123!");

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration);

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var (token, error) = await authService.LoginAsync(request);

            // Assert
            Assert.Null(token);
            Assert.Equal("account_inactive", error);
        }

        [Fact]
        public async Task LoginAsync_ReturnsInvalidPassword_WhenPasswordIsIncorrect()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var existingUser = new User
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            existingUser.HashedPassword = new PasswordHasher<User>()
                .HashPassword(existingUser, "Password123!");

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration);

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "WrongPassword123!"
            };

            // Act
            var (token, error) = await authService.LoginAsync(request);

            // Assert
            Assert.Null(token);
            Assert.Equal("invalid_password", error);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var existingUser = new User
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };

            existingUser.HashedPassword = new PasswordHasher<User>()
                .HashPassword(existingUser, "Password123!");

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration);

            var request = new RefreshTokenReqDto
            {
                UserId = existingUser.Id,
                RefreshToken = "valid-refresh-token"
            };

            // Act
            var result = await authService.RefreshTokensAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));

            // Verify refresh token was rotated
            var updatedUser = await context.Users.FindAsync(existingUser.Id);

            Assert.NotNull(updatedUser);
            Assert.NotEqual("valid-refresh-token", updatedUser!.RefreshToken);
            Assert.Equal(result.RefreshToken, updatedUser.RefreshToken);
            Assert.True(updatedUser.RefreshTokenExpiryTime > DateTime.UtcNow.AddDays(6));
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var authService = new AuthService(context, configuration);

            var request = new RefreshTokenReqDto
            {
                UserId = Guid.NewGuid(),
                RefreshToken = "valid-refresh-token"
            };

            // Act
            var result = await authService.RefreshTokensAsync(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var existingUser = new User
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };

            existingUser.HashedPassword = new PasswordHasher<User>()
                .HashPassword(existingUser, "Password123!");

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration);

            var request = new RefreshTokenReqDto
            {
                UserId = existingUser.Id,
                RefreshToken = "invalid-refresh-token"
            };

            // Act
            var result = await authService.RefreshTokensAsync(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenRefreshTokenHasExpired()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var existingUser = new User
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };

            existingUser.HashedPassword = new PasswordHasher<User>()
                .HashPassword(existingUser, "Password123!");

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration);

            var request = new RefreshTokenReqDto
            {
                UserId = existingUser.Id,
                RefreshToken = "valid-refresh-token"
            };

            // Act
            var result = await authService.RefreshTokensAsync(request);

            // Assert
            Assert.Null(result);
        }
    }
}
