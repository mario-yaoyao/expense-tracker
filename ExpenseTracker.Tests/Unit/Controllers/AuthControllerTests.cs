using ExpenseTracker.API.Controllers;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Common;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseTracker.Tests.Unit.Controllers
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

            var expectedResponse = new ServiceResult<RegisterResDto>
            {
                success = true,
                message = "Registration completed successfully. You can now log in to your account.",
                data = new RegisterResDto
                {
                    UserId = Guid.NewGuid(),
                    FullName = "Test User",
                    Username = "testuser",
                    ContactNumber = "09876543210",
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            mockService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(expectedResponse);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal(expectedResponse.message, response.message);
            Assert.Equal(expectedResponse.data.Username, request.Username);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();

            var request = new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Password = "Password123!",
                ConfirmPassword = "Password456."
            };

            var expectedResponse = new ServiceResult<RegisterResDto>
            {
                success = false,
                message = "Passwords do not match.",
            };

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(expectedResponse);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal(expectedResponse.message, response.message);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameIsTaken()
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

            var expectedResponse = new ServiceResult<RegisterResDto>
            {
                success = false,
                message = "Username is already taken.",
            };

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(expectedResponse);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal(expectedResponse.message, response.message);
        }

        [Fact]
        public async Task Register_Returns500_WhenExceptionOccurs()
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

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                success = false,
                message = "An error occurred while registering account:"
            };

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
            Assert.Contains(expectedResponse.message, response.message);
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

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                success = true,
                message = "Login successful.",
                data = tokenResponse
            };

            mockService.Setup(x => x.LoginAsync(request))
                .ReturnsAsync(new ServiceResult<TokenResDto>
                {
                    success = true,
                    message = "Login successful.",
                    data = tokenResponse
                });

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

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                success = false,
                message = "No account found with that username.",
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync(expectedResponse);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal(expectedResponse.message, response.message);
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

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                success = false,
                message = "Incorrect password.",
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync(expectedResponse);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal(expectedResponse.message, response.message);
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

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                success = false,
                message = "Your account has been deactivated. Please contact support for assistance.",
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync(expectedResponse);

            var controller = new AuthController(mockService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

            Assert.False(response.success);
            Assert.Equal(expectedResponse.message, response.message);
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

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                success = false,
                message = "An error occurred while logging in:"
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
            Assert.Contains(expectedResponse.message, response.message);
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
