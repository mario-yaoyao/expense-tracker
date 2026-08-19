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
            var controller = CreateController(mockService);

            var request = CreateRegisterRequest();

            var expectedResponse = new ServiceResult<RegisterResDto>
            {
                Success = true,
                Data = new RegisterResDto
                {
                    UserId = 1,
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

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(expectedResponse.Data.Username, response.Data!.Username);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreateRegisterRequest();

            var expectedResponse = new ServiceResult<RegisterResDto>
            {
                Success = false,
                ErrorMessage = "Passwords do not match.",
            };

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(badRequestResult.Value);

            Assert.False(response.Success);
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameIsTaken()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreateRegisterRequest();

            var expectedResponse = new ServiceResult<RegisterResDto>
            {
                Success = false,
                ErrorMessage = "Username is already taken.",
            };

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<RegisterResDto>>(badRequestResult.Value);

            Assert.False(response.Success);
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task Register_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreateRegisterRequest();

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                Success = false,
                ErrorMessage = "An error occurred while registering account:"
            };

            mockService
                .Setup(x => x.RegisterAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.Register(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreateLoginRequest();
            var tokenResponse = CreateTokenResponse();

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                Success = true,
                ErrorMessage = "Login successful.",
                Data = tokenResponse
            };

            mockService.Setup(x => x.LoginAsync(request))
                .ReturnsAsync(new ServiceResult<TokenResDto>
                {
                    Success = true,
                    ErrorMessage = "Login successful.",
                    Data = tokenResponse
                });

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(tokenResponse.AccessToken, response.Data!.AccessToken);
            Assert.Equal(tokenResponse.RefreshToken, response.Data.RefreshToken);
        }

        [Theory]
        [InlineData("No account found with that username.")]
        [InlineData("Incorrect password.")]
        [InlineData("Your account has been deactivated. Please contact support for assistance.")]
        public async Task Login_ReturnsBadRequest_WhenServiceReturnsFailure(
            string errorMessage)
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreateLoginRequest();
            var tokenResponse = CreateTokenResponse();

            mockService
                .Setup(x => x.LoginAsync(request))
                .ReturnsAsync(new ServiceResult<TokenResDto>
                {
                    Success = false,
                    ErrorMessage = errorMessage
                });

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequest.Value);

            Assert.False(response.Success);
            Assert.Equal(errorMessage, response.ErrorMessage);
        }

        //[Fact]
        //public async Task Login_ReturnsBadRequest_WhenUserNotFound()
        //{
        //    // Arrange
        //    var mockService = new Mock<IAuthService>();

        //    var request = new LoginUserReqDto
        //    {
        //        Username = "testuser",
        //        Password = "Password123!"
        //    };

        //    var expectedResponse = new ServiceResult<TokenResDto>
        //    {
        //        Success = false,
        //        ErrorMessage = "No account found with that username.",
        //    };

        //    mockService
        //        .Setup(x => x.LoginAsync(request))
        //        .ReturnsAsync(expectedResponse);

        //    var controller = new AuthController(mockService.Object);

        //    // Act
        //    var result = await controller.Login(request);

        //    // Assert
        //    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

        //    Assert.False(response.Success);
        //    Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        //}

        //[Fact]
        //public async Task Login_ReturnsBadRequest_WhenPasswordIsIncorrect()
        //{
        //    // Arrange
        //    var mockService = new Mock<IAuthService>();

        //    var request = new LoginUserReqDto
        //    {
        //        Username = "testuser",
        //        Password = "Password456!"
        //    };

        //    var expectedResponse = new ServiceResult<TokenResDto>
        //    {
        //        Success = false,
        //        ErrorMessage = "Incorrect password.",
        //    };

        //    mockService
        //        .Setup(x => x.LoginAsync(request))
        //        .ReturnsAsync(expectedResponse);

        //    var controller = new AuthController(mockService.Object);

        //    // Act
        //    var result = await controller.Login(request);

        //    // Assert
        //    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

        //    Assert.False(response.Success);
        //    Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        //}

        //[Fact]
        //public async Task Login_ReturnsBadRequest_WhenAccountIsInactive()
        //{
        //    // Arrange
        //    var mockService = new Mock<IAuthService>();

        //    var request = new LoginUserReqDto
        //    {
        //        Username = "testuser",
        //        Password = "Password123!"
        //    };

        //    var expectedResponse = new ServiceResult<TokenResDto>
        //    {
        //        Success = false,
        //        ErrorMessage = "Your account has been deactivated. Please contact support for assistance.",
        //    };

        //    mockService
        //        .Setup(x => x.LoginAsync(request))
        //        .ReturnsAsync(expectedResponse);

        //    var controller = new AuthController(mockService.Object);

        //    // Act
        //    var result = await controller.Login(request);

        //    // Assert
        //    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<TokenResDto>>(badRequestResult.Value);

        //    Assert.False(response.Success);
        //    Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        //}

        [Fact]
        public async Task Login_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreateLoginRequest();

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                Success = false,
                ErrorMessage = "An error occurred while logging in:"
            };

            mockService
                .Setup(x => x.LoginAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.Login(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenRefreshTokenIsValid()
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
        public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenIsInvalid()
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
            Assert.Equal("Invalid refresh token.", response.ErrorMessage);
        }

        [Fact]
        public async Task RefreshToken_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreateRefreshTokenRequest();

            mockService.Setup(x => x.RefreshTokensAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.RefreshToken(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while refreshing token.", response.ErrorMessage);
        }

        // Helper Functions
        private static AuthController CreateController(Mock<IAuthService> mockService)
        {
            return new AuthController(mockService.Object);
        }

        private static RegisterReqDto CreateRegisterRequest()
        {
            return new RegisterReqDto
            {
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
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
    }
}
