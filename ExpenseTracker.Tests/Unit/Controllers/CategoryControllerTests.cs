using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Controllers;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace ExpenseTracker.Tests.Unit.Controllers
{
    public class CategoryControllerTests
    {
        [Fact]
        public async Task GetCategories_ReturnsOk_WhenCategoriesExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var queryRequest = CreateQueryRequest();
            var expectedResponseData = new List<CategoryResDto>
            {
                CreateCategoryResponse(),
                CreateCategoryResponse(name: "Rent")
            };
            var expectedResponse = (
                Data: expectedResponseData,
                HasNextPage: false
            );

            mockService
                .Setup(x => x.GetCategoriesAsync(userId, "User", queryRequest))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetCategories(queryRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<CategoriesResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(expectedResponseData[0].Name, response.Data!.Items[0].Name);
            Assert.Equal(expectedResponseData[1].Type, response.Data!.Items[1].Type);

            mockService.Verify(
                x => x.GetCategoriesAsync(userId, "User", queryRequest),
                Times.Once);
        }

        [Fact]
        public async Task GetCategories_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var queryRequest = CreateQueryRequest();

            mockService.Setup(x => x.GetCategoriesAsync(userId, "User", queryRequest))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetCategories(queryRequest);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving categories.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsOk_WhenCategoryExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var expectedResponse = new CategoryResDto
            {
                Id = categoryId,
                UserId = userId,
                Name = "Rent",
                Type = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockService.Setup(x => x.GetCategoryByIdAsync(userId, "User", categoryId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetCategoryById(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<CategoryResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsNotFound_WhenNoCategoryxist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.GetCategoryByIdAsync(userId, "User", categoryId))
                .ReturnsAsync((CategoryResDto?)null);

            // Act
            var result = await controller.GetCategoryById(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Category not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreateCategory_ReturnsOk_WhenCategoryCreated()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new CreateCategoryReqDto
            {
                Name = "Rent",
                Type = 0,
            };

            var expectedResponse = new CategoryResDto
            {
                Id = 1,
                UserId = userId,
                Name = "Rent",
                Type = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockService.Setup(x => x.CreateCategoryAsync(userId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CreateCategory(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<CategoryResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task CreateCategory_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new CreateCategoryReqDto
            {
                Name = "Rent",
                Type = 0,
            };

            mockService
                .Setup(x => x.CreateCategoryAsync(userId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.CreateCategory(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while creating the category.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsOk_WhenCategoryExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new UpdateCategoryReqDto
            {
                Name = "Rent",
                Type = 0,
            };

            var expectedResponse = new CategoryResDto
            {
                Id = categoryId,
                UserId = userId,
                Name = "Rent",
                Type = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            mockService.Setup(x => x.UpdateCategoryAsync(userId, categoryId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.UpdateCategory(categoryId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<CategoryResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(expectedResponse.Name, response.Data!.Name);
            Assert.Equal(expectedResponse.Type, response.Data!.Type);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNotFound_WhenNoCategoryExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new UpdateCategoryReqDto
            {
                Name = "Rent",
                Type = 0,
            };

            mockService.Setup(x => x.UpdateCategoryAsync(userId, categoryId, request))
                .ReturnsAsync((CategoryResDto?)null);

            // Act
            var result = await controller.UpdateCategory(categoryId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Category not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateCategory_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new UpdateCategoryReqDto
            {
                Name = "Rent",
                Type = 0,
            };

            mockService.Setup(x => x.UpdateCategoryAsync(userId, categoryId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.UpdateCategory(categoryId, request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while updating the category.", response.ErrorMessage);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsOk_WhenCategoryExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteCategoryAsync(userId, categoryId))
                .ReturnsAsync(true);

            // Act
            var result = await controller.DeleteCategory(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<CategoryResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNotFound_WhenNoCategoryExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteCategoryAsync(userId, categoryId))
                .ReturnsAsync(false);

            // Act
            var result = await controller.DeleteCategory(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Category not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task DeleteCategory_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteCategoryAsync(userId, categoryId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.DeleteCategory(categoryId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while deleting the category.", response.ErrorMessage);
        }

        // Helper Functions
        private static void SetUserClaims(ControllerBase controller, int userId, string role)
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

        private static CategoryController CreateController(Mock<ICategoryService> mockService)
        {
            return new CategoryController(mockService.Object);
        }

        private static CategoryResDto CreateCategoryResponse(
            int id = 1,
            int userId = 1,
            string name = "Utilities",
            CategoryType type = CategoryType.Expense,
            bool isDeleted = false)
        {
            return new CategoryResDto
            {
                Id = id,
                UserId = userId,
                Name = name,
                Type = type,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static CategoryQueryReqDto CreateQueryRequest(
            CategoryType type = CategoryType.Expense,
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new CategoryQueryReqDto
            {
                Type = type,
                Page = page,
                Limit = limit,
                Search = search
            };
        }
    }
}
