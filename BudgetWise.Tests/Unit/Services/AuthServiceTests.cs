using BudgetWise.BLL.Interfaces;
using BudgetWise.BLL.Services;
using BudgetWise.Controllers;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text.Json;

namespace BudgetWise.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegisterAsync_ReturnsSuccess_WhenRegistrationIsValid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var registerReq = CreateRegisterRequest();

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(false);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(registerReq));

            // Act
            var result = await authService.RegisterAsync(encryptedReq);

            // Assert
            Assert.True(result.Success);

            mockRepo.Verify(
                x => x.AddUserAsync(It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var registerReq = CreateRegisterRequest(confirmPassword: "Password456!");

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(true);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(registerReq));

            // Act
            var result = await authService.RegisterAsync(encryptedReq);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Passwords do not match.", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenUsernameIsTaken()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var registerReq = CreateRegisterRequest();

            mockRepo.Setup(x => x.IsUsernameTakenAsync("testuser"))
                .ReturnsAsync(true);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(registerReq));

            // Act
            var result = await authService.RegisterAsync(encryptedReq);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username is already taken.", result.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_ReturnsSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var loginRequest = CreateLoginRequest();
            var user = CreateUser();

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, "Password123!");

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(user);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(loginRequest));

            // Act
            var result = await authService.LoginAsync(encryptedReq);

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
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var loginReq = CreateLoginRequest();
            var user = CreateUser(isActive: false);

            var expectedResponse = new User
            {
                Id = user.Id,
                Username = user.Username,
                HashedPassword = user.HashedPassword,
                IsActive = user.IsActive
            };

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(expectedResponse);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(loginReq));

            // Act
            var result = await authService.LoginAsync(encryptedReq);

            // Assert
            Assert.False(result.Success);
            Assert.False(expectedResponse.IsActive);
            Assert.Equal("Your account has been deactivated.", result.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_ReturnsFailure_WhenPasswordIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var loginReq = CreateLoginRequest(password: "DifferentPassword123!");
            var user = CreateUser();

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync(user);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(loginReq));

            // Act
            var result = await authService.LoginAsync(encryptedReq);

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
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var loginReq = CreateLoginRequest();

            mockRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                .ReturnsAsync((User?)null);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(loginReq));

            // Act
            var result = await authService.LoginAsync(encryptedReq);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No account found with that username.", result.ErrorMessage);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsToken_WhenRefreshTokenValid()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var mockCryptoService = new Mock<ICryptoService>();
            var controller = CreateController(mockAuthService);

            var encryptedReq = CreatedEncryptedRequest();
            var refreshReq = CreateRefreshTokenRequest();
            var tokenRes = CreateTokenResponse();

            mockAuthService.Setup(x => x.RefreshTokensAsync(encryptedReq))
                .ReturnsAsync(tokenRes);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(refreshReq));

            // Act
            var result = await controller.RefreshToken(encryptedReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(refreshReq.RefreshToken, response.Data.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var mockCryptoService = new Mock<ICryptoService>();
            var controller = CreateController(mockAuthService);

            var encryptedReq = CreatedEncryptedRequest();
            var refreshReq = CreateRefreshTokenRequest();
            var tokenRes = CreateTokenResponse();

            mockAuthService.Setup(x => x.RefreshTokensAsync(encryptedReq))
                .ReturnsAsync((tokenRes));

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(refreshReq));

            // Act
            var result = await controller.RefreshToken(encryptedReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(tokenRes.AccessToken, response.Data!.AccessToken);
            Assert.Equal(tokenRes.RefreshToken, response.Data.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokensAsync_ReturnsNull_WhenRefreshTokenDoesNotMatch()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var mockCryptoService = new Mock<ICryptoService>();
            var controller = CreateController(mockAuthService);

            var encryptedReq = CreatedEncryptedRequest();
            var refreshReq = CreateRefreshTokenRequest();
            var tokenRes = CreateTokenResponse();

            mockAuthService.Setup(x => x.RefreshTokensAsync(encryptedReq))
                .ReturnsAsync((TokenResDto?)null);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(refreshReq));

            // Act
            var result = await controller.RefreshToken(encryptedReq);

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
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var refreshReq = CreateRefreshTokenRequest();
            var user = CreateUser(refreshToken: "test-refresh-token", refreshTokenExpiryTime: DateTime.UtcNow.AddDays(-1));

            mockRepo.Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(refreshReq));

            // Act
            var result = await authService.RefreshTokensAsync(encryptedReq);

            // Assert
            Assert.Null(result);
        }

        //
        [Fact]
        public async Task ResetPasswordAsync_ReturnsSuccess_WhenUpdatePasswordSucceeds()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var resetReq = CreateResetPasswordRequest();
            var user = CreateUser(resetToken: resetReq.Token, resetTokenExpiryTime: DateTime.UtcNow.AddMinutes(5));

            mockRepo.Setup(x => x.GetUserByResetToken(resetReq.Token))
                .ReturnsAsync(user);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(resetReq));

            // Act
            var result = await authService.ResetPasswordAsync(encryptedReq);

            // Assert
            Assert.True(result.Success);

            mockRepo.Verify(
                x => x.GetUserByResetToken(resetReq.Token),
                Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenPasswordsDoesNotMatch()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var resetReq = CreateResetPasswordRequest(confirmNewPassword: "password789");
            var user = CreateUser();

            mockRepo.Setup(x => x.GetUserByResetToken(resetReq.Token))
                .ReturnsAsync(user);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(resetReq));

            // Act
            var result = await authService.ResetPasswordAsync(encryptedReq);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Passwords do not match.", result.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenResetTokenIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var resetReq = CreateResetPasswordRequest();

            mockRepo.Setup(x => x.GetUserByResetToken(resetReq.Token))
                .ReturnsAsync((User?)null);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(resetReq));

            // Act
            var result = await authService.ResetPasswordAsync(encryptedReq);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid reset token.", result.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFailure_WhenResetTokenIsExpired()
        {
            // Arrange
            var mockRepo = new Mock<IAuthRepository>();
            var mockCryptoService = new Mock<ICryptoService>();
            var authService = CreateAuthService(mockRepo, mockCryptoService: mockCryptoService);

            var encryptedReq = CreatedEncryptedRequest();
            var resetReq = CreateResetPasswordRequest();
            var user = CreateUser(resetToken: resetReq.Token, resetTokenExpiryTime: DateTime.UtcNow.AddMinutes(-1));

            mockRepo.Setup(x => x.GetUserByResetToken(resetReq.Token))
                .ReturnsAsync(user);

            mockCryptoService
                .Setup(x => x.Decrypt(It.IsAny<string>()))
                .Returns(JsonSerializer.Serialize(resetReq));

            // Act
            var result = await authService.ResetPasswordAsync(encryptedReq);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Reset token has expired. Please submit a new password reset request.", result.ErrorMessage);
        }

        //Helper Functions
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
            Mock<IAuthRepository>? mockRepo = null,
            Mock<IEmailService>? mockEmailService = null,
            Mock<ICryptoService>? mockCryptoService = null)
        {
            mockRepo ??= new Mock<IAuthRepository>();
            mockEmailService ??= new Mock<IEmailService>();
            mockCryptoService ??= new Mock<ICryptoService>();

            return new AuthService(
                CreateConfiguration(),
                mockRepo.Object,
                mockEmailService.Object,
                mockCryptoService.Object);
        }

        private static AuthController CreateController(Mock<IAuthService> mockService)
        {
            return new AuthController(mockService.Object);
        }

        private static User CreateUser(
            int id = 1,
            string username = "testuser",
            string refreshToken = "test-refresh-token",
            DateTime? refreshTokenExpiryTime = null,
            string resetToken = "test-reset-token",
            DateTime? resetTokenExpiryTime = null,
            bool isActive = true,
            DateTime? createdAt = null)
        {
            var user = new User
            {
                Id = id,
                FullName = "Test User",
                Username = username,
                ContactNumber = "09123456789",
                Role = UserRole.User,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = refreshTokenExpiryTime,
                ResetToken = resetToken,
                ResetTokenExpiryTime = resetTokenExpiryTime,
                IsActive = isActive,
                CreatedAt = createdAt ?? DateTime.Now,
            };

            user.HashedPassword = new PasswordHasher<User>()
                .HashPassword(user, "Password123!");

            return user;
        }

        private static EncryptedReqDto CreatedEncryptedRequest()
        {
            return new EncryptedReqDto
            {
                EncryptedData = "randomEncryptedString"
            };
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

        private static LoginReqDto CreateLoginRequest(
            string username = "testuser",
            string password = "Password123!")
        {
            return new LoginReqDto
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
