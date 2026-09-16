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
            SetUserClaims(controller);

            var queryReq = CreateQueryRequest();

            var mappedCategories = new List<CategoryResDto>
            {
                CreateCategoryResponse(),
                CreateCategoryResponse(name: "Rent")
            };

            var expectedResponse = (
                Data: mappedCategories,
                HasNextPage: false
            );

            mockService
                .Setup(x => x.GetCategoriesAsync(userId, "User", queryReq))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetCategories(queryReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<CategoriesResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(mappedCategories[0].Name, response.Data!.Items[0].Name);
            Assert.Equal(mappedCategories[1].Type, response.Data!.Items[1].Type);

            mockService.Verify(
                x => x.GetCategoriesAsync(userId, "User", queryReq),
                Times.Once);
        }

        [Fact]
        public async Task GetCategories_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var queryReq = CreateQueryRequest();

            mockService.Setup(x => x.GetCategoriesAsync(userId, "User", queryReq))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetCategories(queryReq);

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
            SetUserClaims(controller);

            var expectedResponse = CreateCategoryResponse();

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
            SetUserClaims(controller);

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
            SetUserClaims(controller);

            var request = CreateCategory();

            mockService.Setup(x => x.CreateCategoryAsync(userId, request))
                .ReturnsAsync(true);

            // Act
            var result = await controller.CreateCategory(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task CreateCategory_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var request = CreateCategory();

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
            SetUserClaims(controller);

            var request = UpdateCategory();

            mockService.Setup(x => x.UpdateCategoryAsync(userId, categoryId, request))
                .ReturnsAsync(true);

            // Act
            var result = await controller.UpdateCategory(categoryId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNotFound_WhenNoCategoryExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ICategoryService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var request = UpdateCategory();

            mockService.Setup(x => x.UpdateCategoryAsync(userId, categoryId, request))
                .ReturnsAsync(false);

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
            SetUserClaims(controller);

            var request = UpdateCategory();

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
            SetUserClaims(controller);

            mockService.Setup(x => x.DeleteCategoryAsync(userId, categoryId))
                .ReturnsAsync(true);

            // Act
            var result = await controller.DeleteCategory(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

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
            SetUserClaims(controller);

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
            SetUserClaims(controller);

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

        private static CategoryController CreateController(Mock<ICategoryService> mockService)
        {
            return new CategoryController(mockService.Object);
        }

        private static CreateCategoryReqDto CreateCategory(
            string name = "Rent",
            CategoryType type = CategoryType.Expense)
        {
            return new CreateCategoryReqDto
            {
                Name = name,
                Type = type
            };
        }

        private static UpdateCategoryReqDto UpdateCategory(
            string name = "Rent",
            CategoryType type = CategoryType.Expense)
        {
            return new UpdateCategoryReqDto
            {
                Name = name,
                Type = type
            };
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
