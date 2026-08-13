using ExpenseTracker.API.Controllers;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegisterAsync_ReturnsSuccess_WhenRegistrationIsValid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(false);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new AuthService(configuration, mockRepo.Object);

            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(request.Username, result.Data.Username);

            mockRepo.Verify(
                x => x.AddUserAsync(It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(true);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new AuthService(configuration, mockRepo.Object);

            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Password = "Password123!",
                ConfirmPassword = "Password456!"
            };
            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Passwords do not match.", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenUsernameIsTaken()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(true);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new AuthService(configuration, mockRepo.Object);

            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };
            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username is already taken.", result.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_ReturnsSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                IsActive = true
            };

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, "Password123!");

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(user);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new AuthService(configuration, mockRepo.Object);

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var result = await service.LoginAsync(request);

            // Assert
            Assert.True(result.Success);

            mockRepo.Verify(
                x => x.GetByUsernameAsync("testuser"),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ReturnsFailure_WhenAccountIsInactive()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();

            var expectedResponse = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                HashedPassword = "hashed-password",
                IsActive = false
            };

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(expectedResponse);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new AuthService(configuration, mockRepo.Object);

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var result = await service.LoginAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Your account has been deactivated.", result.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_ReturnsFailure_WhenPasswordIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                IsActive = true
            };

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, "Password123!");

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(user);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new AuthService(configuration, mockRepo.Object);

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "DifferentPassword123!"
            };

            // Act
            var result = await service.LoginAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Incorrect password.", result.ErrorMessage);

            mockRepo.Verify(
                x => x.GetByUsernameAsync("testuser"),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ReturnsError_WhenUserDoesNotExist()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync((User?)null);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();

            var service = new AuthService(configuration, mockRepo.Object);

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var result = await service.LoginAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No account found with that username.", result.ErrorMessage);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsToken_WhenRefreshTokenValid()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new RefreshTokenReqDto
            {
                UserId = Guid.NewGuid(),
                RefreshToken = "test-refresh-token"
            };

            var tokenResponse = new TokenResDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token"
            };

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ReturnsAsync(tokenResponse);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.RefreshToken(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(request.RefreshToken, response.Data.RefreshToken);
        }
        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new RefreshTokenReqDto
            {
                UserId = Guid.NewGuid(),
                RefreshToken = "test-refresh-token"
            };

            var tokenResponse = new TokenResDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token"
            };

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ReturnsAsync((tokenResponse));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.RefreshToken(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(tokenResponse.AccessToken, response.Data!.AccessToken);
            Assert.Equal(tokenResponse.RefreshToken, response.Data.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenRefreshTokenDoesNotMatch()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new RefreshTokenReqDto
            {
                UserId = Guid.NewGuid(),
                RefreshToken = "test-different-refresh-token"
            };

            var tokenResponse = new TokenResDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token"
            };

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ReturnsAsync((TokenResDto?)null);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.RefreshToken(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(unauthorizedResult.Value);

            Assert.False(response.Success);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenRefreshTokenExpired()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockConfig = new Mock<IConfiguration>();

            var user = new User
            {
                Id = Guid.NewGuid(),
                RefreshToken = "test-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };

            mockRepo.Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            var authService = new AuthService(
                mockConfig.Object,
                mockRepo.Object);

            var request = new RefreshTokenReqDto
            {
                UserId = user.Id,
                RefreshToken = "test-refresh-token"
            };

            // Act
            var result = await authService.RefreshTokensAsync(request);

            // Assert
            Assert.Null(result);
        }
    }
}
