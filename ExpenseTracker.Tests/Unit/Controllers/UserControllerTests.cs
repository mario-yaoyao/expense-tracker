using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Controllers;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace ExpenseTracker.Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        [Fact]
        public async Task GetUserProfile_ReturnsOk_WhenUserExist()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var userId = Guid.NewGuid();

            var expectedResponse = new UserResDto
            {
                Id = userId,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockService
                .Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync(expectedResponse);

            var controller = new UserController(mockService.Object);
            SetUserClaims(controller, userId, "User");

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
            var mockService = new Mock<IUserService>();
            var userId = Guid.NewGuid();

            var expectedResponse = new UserResDto();

            mockService
                .Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync((UserResDto?)null);

            var controller = new UserController(mockService.Object);
            SetUserClaims(controller, userId, "User");

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
            var userId = Guid.NewGuid();
            var mockService = new Mock<IUserService>();

            mockService.Setup(x => x.GetUserProfileAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new UserController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving user information.", response.ErrorMessage);
        }

        private static void SetUserClaims(ControllerBase controller, Guid userId, string role)
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
    }
}
