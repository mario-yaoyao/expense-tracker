using ExpenseTracker.Controllers;
using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseTracker.Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var expectedResponse = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            mockService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync((expectedResponse, null));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Registration completed successfully. You can now log in to your account.", response.message);
            Assert.Equal(expectedResponse.Username, response.data!.Username);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                Password = "Password123",
                ConfirmPassword = "Password456"
            };

            var mockService = new Mock<IAuthService>();

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ReturnsAsync((null, "password_mismatch"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal("Passwords do not match.", response.message);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameIsTaken()
        {
            // Arrange
            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var mockService = new Mock<IAuthService>();

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ReturnsAsync((null, "duplicate_name"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal("Username is already taken.", response.message);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_ForUnknownError()
        {
            // Arrange
            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var mockService = new Mock<IAuthService>();

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ReturnsAsync((null, "unexpected_error"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal("Registration failed.", response.message);
        }

        [Fact]
        public async Task Register_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var mockService = new Mock<IAuthService>();

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while registering account:", response.message);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var tokenResponse = new TokenResDto
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token"
            };

            mockService.Setup(x => x.LoginAsync(request))
                .ReturnsAsync((tokenResponse, null));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal(tokenResponse.AccessToken, response.data!.AccessToken);
            Assert.Equal(tokenResponse.RefreshToken, response.data.RefreshToken);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenUserNotFound()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync((null, "user_not_found"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal("No account found with that username.", response.message);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenPasswordIsIncorrect()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password456!"
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync((null, "invalid_password"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal("Incorrect password.", response.message);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenAccountIsInactive()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync((null, "account_inactive"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal("Your account has been deactivated. Please contact support for assistance.", response.message);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_ForUnknownError()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync((null, "unexpected_error"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal("Login failed.", response.message);
        }

        [Fact]
        public async Task Login_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new LoginUserReqDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while logging in:", response.message);
        }

        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenRefreshTokenIsValid()
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

            Assert.True(response.success);
            Assert.Equal(tokenResponse.AccessToken, response.data!.AccessToken);
            Assert.Equal(tokenResponse.RefreshToken, response.data.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new RefreshTokenReqDto
            {
                UserId = Guid.NewGuid(),
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

            Assert.False(response.success);
            Assert.Equal("Invalid refresh token.", response.message);
        }

        [Fact]
        public async Task RefreshToken_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new RefreshTokenReqDto
            {
                UserId = Guid.NewGuid(),
                RefreshToken = "test-refresh-token"
            };

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.RefreshToken(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while refreshing token.", response.message);
        }
    }
}
