using BudgetWise.Controllers;
using BudgetWise.BLL.Interfaces;
using BudgetWise.Models.Common;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BudgetWise.Tests.Unit.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<object>
            {
                Success = true
            };

            mockService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<object>
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
            var response = Assert.IsType<ApiResDto<object>>(badRequestResult.Value);

            Assert.False(response.Success);
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameIsTaken()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<object>
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
            var response = Assert.IsType<ApiResDto<object>>(badRequestResult.Value);

            Assert.False(response.Success);
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task Register_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                Success = false,
                ErrorMessage = "An error occurred while registering account."
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

            var request = CreatedEncryptedRequest();
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

            var request = CreatedEncryptedRequest();
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

        [Fact]
        public async Task Login_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                Success = false,
                ErrorMessage = "An error occurred while logging in."
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
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenRefreshTokenIsValid()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();
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

            var request = CreatedEncryptedRequest();
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

            var request = CreatedEncryptedRequest();

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

        [Fact]
        public async Task ForgotPassword_ReturnsOk_WhenUpdatePasswordSucceeds()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            mockService.Setup(x => x.ForgotPasswordAsync(request))
                .ReturnsAsync(true);

            // Act
            var result = await controller.ForgotPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsNotFound_WhenAccountDoesNotExist()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ApiResDto<object>
            {
                Success = false,
                ErrorMessage = "Passwords do not match.",
            };

            mockService
                .Setup(x => x.ForgotPasswordAsync(request))
                .ReturnsAsync(false);

            // Act
            var result = await controller.ForgotPassword(request);

            // Assert
            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(badRequestResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Account not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task ForgotPassword_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<TokenResDto>
            {
                Success = false,
                ErrorMessage = "An error occurred while resetting password."
            };

            mockService
                .Setup(x => x.ForgotPasswordAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.ForgotPassword(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task ResetPassword_ReturnsOk_WhenUpdatePasswordSucceeds()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<object>
            {
                Success = true,
                Data = true
            };

            mockService.Setup(x => x.ResetPasswordAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.ChangePassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task ResetPassword_ReturnsNotFound_WhenPasswordDoesNotMatch()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<object>
            {
                Success = false,
                ErrorMessage = "Passwords do not match.",
            };

            mockService
                .Setup(x => x.ResetPasswordAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.ChangePassword(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(badRequestResult.Value);

            Assert.False(response.Success);
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task ResetPassword_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = CreateController(mockService);

            var request = CreatedEncryptedRequest();

            var expectedResponse = new ServiceResult<object>
            {
                Success = false,
                ErrorMessage = "An error occurred while resetting password."
            };

            mockService
                .Setup(x => x.ResetPasswordAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.ChangePassword(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal(expectedResponse.ErrorMessage, response.ErrorMessage);
        }

        // Helper Functions
        private static AuthController CreateController(Mock<IAuthService> mockService)
        {
            return new AuthController(mockService.Object);
        }

        private static EncryptedReqDto CreatedEncryptedRequest()
        {
            return new EncryptedReqDto
            {
                EncryptedData = "randomEncryptedString"
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
