using BudgetWise.BLL.Interfaces;
using BudgetWise.Controllers;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace BudgetWise.Tests.Unit.Controllers
{
    public class UserControllerTests
    {
        [Fact]
        public async Task GetUsers_ReturnsOk_WhenUsersExist()
        {
            // Arrange
            var secondUserId = 2;

            var mockService = new Mock<IUserService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var paginationReq = CreatePaginationRequest();
            var mappedUsers = new List<UserResDto>
            {
                CreateUserResponse(),
                CreateUserResponse(id: secondUserId, username: "testusertwo"),
            };

            var expectedResponse = (
                Data: mappedUsers,
                TotalCount: 2,
                HasNextPage: false
            );

            mockService
                .Setup(x => x.GetUsersAsync(paginationReq))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetUsers(paginationReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<UsersResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(mappedUsers[0].Username, response.Data!.Items[0].Username);
            Assert.Equal(mappedUsers[1].Email, response.Data!.Items[1].Email);
            Assert.Equal(mappedUsers[1].ContactNumber, response.Data.Items[1].ContactNumber);

            mockService.Verify(
                x => x.GetUsersAsync(paginationReq),
                Times.Once);
        }

        [Fact]
        public async Task GetUsers_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var paginationReq = CreatePaginationRequest();

            mockService.Setup(x => x.GetUsersAsync(paginationReq))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetUsers(paginationReq);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving users.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IUserService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var expectedResponse = CreateUserResponse();

            mockService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<UserResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenNoUserExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IUserService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            mockService.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((UserResDto?)null);

            // Act
            var result = await controller.GetUserById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("User not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task ToggleUserStatus_ReturnsOk_WhenUserExist()
        {
            // Arrange
            var userId = 1;
            var username = "testuser";

            var mockService = new Mock<IUserService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            mockService.Setup(x => x.ToggleUserStatusAsync(username, userId))
                .ReturnsAsync(true);

            // Act
            var result = await controller.ToggleUserStatus(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task ToggleUserStatus_ReturnsNotFound_WhenNoUserExist()
        {
            // Arrange
            var userId = 1;
            var username = "testuser";

            var mockService = new Mock<IUserService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            mockService.Setup(x => x.ToggleUserStatusAsync(username, userId))
                .ReturnsAsync(false);

            // Act
            var result = await controller.ToggleUserStatus(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("User not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task ToggleUserStatus_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            var username = "testuser";

            var mockService = new Mock<IUserService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            mockService.Setup(x => x.ToggleUserStatusAsync(username, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.ToggleUserStatus(userId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while deactivating a user account.", response.ErrorMessage);
        }

        // Helper Functions
        private static void SetUserClaims(ControllerBase controller, int userId = 1, string username = "testuser", string role = "SuperAdmin")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
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

        private static UserController CreateController(Mock<IUserService> mockService)
        {
            return new UserController(mockService.Object);
        }

        private static UserQueryReqDto CreatePaginationRequest(
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new UserQueryReqDto
            {
                Page = page,
                Limit = limit,
                Search = search
            };
        }

        private static UserResDto CreateUserResponse(
            int id = 1,
            string username = "testuser")
        {
            return new UserResDto
            {
                Id = id,
                Username = username,
                FullName = "Test User",
                Email = "testuser@gmail.com",
                ContactNumber = "09123456789",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
