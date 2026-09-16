using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Controllers;
using ExpenseTracker.Models.Common;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace ExpenseTracker.Tests.Unit.Controllers
{
    public class ProfileControllerTests
    {
        [Fact]
        public async Task GetUserProfile_ReturnsOk_WhenUserExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IProfileService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var expectedResponse = CreateUserResponse();

            mockService
                .Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<UserResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(expectedResponse.Username, response.Data.Username);
            Assert.Equal(expectedResponse.Id, response.Data.Id);
        }

        [Fact]
        public async Task GetUserProfile_ReturnsNotFound_WhenNoUserExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IProfileService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var expectedResponse = new UserResDto();

            mockService
                .Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync((UserResDto?)null);

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("User information not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetUserProfile_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IProfileService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            mockService.Setup(x => x.GetUserProfileAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving user information.", response.ErrorMessage);
        }

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenPasswordChangedSuccessfully()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IProfileService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var request = ChangePasswordRequest();

            var expectedResponse = new ServiceResult<object>
            {
                Success = true
            };

            mockService
                .Setup(x => x.ChangePasswordAsync(userId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.ChangePassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IProfileService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var request = ChangePasswordRequest();

            var expectedResponse = new ServiceResult<object>
            {
                Success = false,
                ErrorMessage = "Current password is incorrect."
            };

            mockService
                .Setup(x => x.ChangePasswordAsync(userId, request))
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
        public async Task ChangePassword_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IProfileService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var request = ChangePasswordRequest();

            mockService
                .Setup(x => x.ChangePasswordAsync(userId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.ChangePassword(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while changing password.", response.ErrorMessage);
        }

        // Helper Functions
        private static void SetUserClaims(ControllerBase controller, int userId = 1, string role = "User")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(claims, "Test"))
                }
            };
        }

        private static ProfileController CreateController(Mock<IProfileService> mockService)
        {
            return new ProfileController(mockService.Object);
        }

        private static ChangePasswordReqDto ChangePasswordRequest()
        {
            return new ChangePasswordReqDto
            {
                CurrentPassword = "oldpassword123",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };
        }

        private static UserResDto CreateUserResponse(
            int userId = 1,
            DateTime? createdAt = null)
        {
            return new UserResDto
            {
                Id = userId,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = createdAt ?? new DateTime(2026, 1, 1),
                UpdatedAt = null
            };
        }
    }
}
