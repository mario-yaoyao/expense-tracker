using ExpenseTracker.Controllers;
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
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateRegisterRequest();

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(false);

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
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateRegisterRequest(confirmPassword: "Password456!");

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(true);

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
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateRegisterRequest();

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(true);

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
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateLoginRequest();

            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                IsActive = true
            };

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, "Password123!");

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(user);

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
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateLoginRequest();

            var expectedResponse = new User
            {
                Id = 1,
                Username = "testuser",
                HashedPassword = "hashed-password",
                IsActive = false
            };

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(expectedResponse);

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
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateLoginRequest(password: "DifferentPassword123!");

            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                IsActive = true
            };

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, "Password123!");

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(user);

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
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateLoginRequest();

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync((User?)null);

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
            var controller = CreateController(mockService);

            var request = CreateRefreshTokenRequest();
            var tokenResponse = CreateTokenResponse();

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ReturnsAsync(tokenResponse);

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
            var controller = CreateController(mockService);

            var request = CreateRefreshTokenRequest();
            var tokenResponse = CreateTokenResponse();

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ReturnsAsync((tokenResponse));

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
            var controller = CreateController(mockService);

            var request = CreateRefreshTokenRequest();
            var tokenResponse = CreateTokenResponse();

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ReturnsAsync((TokenResDto?)null);

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
            var mockEmailService = new Mock<IEmailService>();

            var request = CreateRefreshTokenRequest();

            var user = new User
            {
                Id = 1,
                RefreshToken = "test-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };

            mockRepo.Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            var authService = new AuthService(
                mockConfig.Object,
                mockRepo.Object,
                mockEmailService.Object);

            // Act
            var result = await authService.RefreshTokensAsync(request);

            // Assert
            Assert.Null(result);
        }

        //
        [Fact]
        public async Task ResetPasswordAsync_ReturnsSuccess_WhenUpdatePasswordSucceeds()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateResetPasswordRequest();

            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                IsActive = true,
                ResetToken = request.Token,
                ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(5),
                HashedPassword = "old-password"
            };

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, "Password123!");

            mockRepo.Setup(x => x.GetUserByResetToken(request.Token))
                .ReturnsAsync(user);

            // Act
            var result = await service.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);

            mockRepo.Verify(
                x => x.GetUserByResetToken(request.Token),
                Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenPasswordsDoesNotMatch()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateResetPasswordRequest(confirmNewPassword: "password789");

            var expectedResponse = new User
            {
                Id = 1,
                Username = "testuser",
                HashedPassword = "hashed-password",
                IsActive = false
            };

            mockRepo.Setup(x => x.GetUserByResetToken(request.Token))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await service.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Passwords do not match.", result.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenResetTokenIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateResetPasswordRequest();

            mockRepo.Setup(x => x.GetUserByResetToken(request.Token))
                .ReturnsAsync((User?)null);

            // Act
            var result = await service.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid reset token.", result.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenResetTokenIsExpired()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var service = CreateAuthService(mockRepo, mockEmailService);

            var request = CreateResetPasswordRequest();

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                ResetToken = request.Token,
                ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1)
            };

            mockRepo.Setup(x => x.GetUserByResetToken(request.Token))
                .ReturnsAsync(user);

            // Act
            var result = await service.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Reset token has expired. Please submit a new password reset request.", result.ErrorMessage);
        }

        // Helper Functions
        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppSettings:Token"] = "mK9#xV2!pL7$qR4@tY8^nH3&zW6*eC1+sF5=uJ0~dA9%gB2?oN7!rT4#kP8@vX3&yL6*cD1+sM5=uQ0~hZ9%fG2",
                    ["AppSettings:Issuer"] = "TestIssuer",
                    ["AppSettings:Audience"] = "TestAudience"
                })
                .Build();
        }

        private static AuthService CreateAuthService(
            Mock<IAuthRepository> mockRepo,
            Mock<IEmailService> mockEmailService)
        {
            return new AuthService(
                CreateConfiguration(),
                mockRepo.Object,
                mockEmailService.Object);
        }

        private static AuthController CreateController(Mock<IAuthService> mockService)
        {
            return new AuthController(mockService.Object);
        }

        private static RegisterReqDto CreateRegisterRequest(
            string fullName = "Test User",
            string username = "testuser",
            string contactNumber = "09876543210",
            string password = "Password123!",
            string confirmPassword = "Password123!"
            )
        {
            return new RegisterReqDto
            {
                FullName = fullName,
                Username = username,
                ContactNumber = contactNumber,
                Password = password,
                ConfirmPassword = confirmPassword
            };
        }

        private static LoginUserReqDto CreateLoginRequest(
            string username = "testuser",
            string password = "Password123!")
        {
            return new LoginUserReqDto
            {
                Username = username,
                Password = password
            };
        }

        private static RefreshTokenReqDto CreateRefreshTokenRequest()
        {
            return new RefreshTokenReqDto
            {
                UserId = 1,
                RefreshToken = "test-refresh-token"
            };
        }

        private static TokenResDto CreateTokenResponse()
        {
            return new TokenResDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token"
            };
        }

        private static ResetPasswordReqDto CreateResetPasswordRequest(
            string token = "Qm8vLkzTpF7Wa2+sdN5HrJxCUG9myEbq14KoYVnRDtA=",
            string newPassword = "password456",
            string confirmNewPassword = "password456")
        {
            return new ResetPasswordReqDto
            {
                Token = token,
                NewPassword = newPassword,
                ConfirmNewPassword = confirmNewPassword
            };
        }
    }
}
