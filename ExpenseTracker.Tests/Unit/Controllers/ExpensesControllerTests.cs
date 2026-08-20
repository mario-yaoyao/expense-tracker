using ExpenseTracker.API.Controllers;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace ExpenseTracker.Tests.Unit.Controllers
{
    public class ExpensesControllerTests
    {
        [Fact]
        public async Task GetExpenses_ReturnsOk_WhenExpensesExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var expectedResponse = new List<ExpenseResDto>
            {
                new()
                {
                    Id = 1,
                    UserId = userId,
                    Description = "Breakfast",
                    Amount = 24.99m,
                    CategoryName = "Food",
                    CategoryType = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                },
                new()
                {
                    Id = 2,
                    UserId = userId,
                    Description = "Lunch",
                    Amount = 29.50m,
                    CategoryName = "Food",
                    CategoryType = 0,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                }
            };

            mockService
                .Setup(x => x.GetExpensesAsync(userId, "User"))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<List<ExpenseResDto>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(2, response.Data!.Count);
            Assert.Equal(expectedResponse[0].Description, response.Data[0].Description);
            Assert.Equal(expectedResponse[1].Amount, response.Data[1].Amount);

            mockService.Verify(
                x => x.GetExpensesAsync(userId, "User"),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenses_ReturnsNotFound_WhenNoExpensesExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var expectedResponse = new List<ExpenseResDto>();

            mockService.Setup(x => x.GetExpensesAsync(userId, "User"))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("No expenses found.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetExpenses_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.GetExpensesAsync(userId, "User"))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving expenses.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetExpenseById_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var expectedResponse = new ExpenseResDto
            {
                Id = expenseId,
                UserId = userId,
                Description = "Food",
                Amount = 50.00m,
                CategoryName = "Food",
                CategoryType = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockService.Setup(x => x.GetExpenseByIdAsync(userId, "User", expenseId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetExpenseById(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task GetExpenseById_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.GetExpenseByIdAsync(userId, "User", expenseId))
                .ReturnsAsync((ExpenseResDto?)null);

            // Act
            var result = await controller.GetExpenseById(expenseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Expense not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreateExpense_ReturnsOk_WhenExpenseCreated()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new CreateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                CategoryId = categoryId
            };

            var expectedResponse = new ExpenseResDto
            {
                Id = 1,
                UserId = userId,
                Description = "Food"
            };

            mockService.Setup(x => x.CreateExpenseAsync(userId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CreateExpense(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task CreateExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new CreateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                CategoryId = 1
            };

            mockService
                .Setup(x => x.CreateExpenseAsync(userId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.CreateExpense(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while creating the expense.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new UpdateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                CategoryId = 1
            };

            var expectedResponse = new ExpenseResDto
            {
                Id = expenseId,
                UserId = userId,
                Description = "Lunch",
                Amount = 50.00m,
                CategoryName = "Food",
                CategoryType = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            mockService.Setup(x => x.UpdateExpenseAsync(userId, "User", expenseId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.UpdateExpense(expenseId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(expectedResponse.Description, response.Data!.Description);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new UpdateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                CategoryId = 1
            };

            mockService.Setup(x => x.UpdateExpenseAsync(userId, "User", expenseId, request))
                .ReturnsAsync((ExpenseResDto?)null);

            // Act
            var result = await controller.UpdateExpense(expenseId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Expense not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = new UpdateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                CategoryId = 1
            };

            mockService.Setup(x => x.UpdateExpenseAsync(userId, "User", expenseId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.UpdateExpense(expenseId, request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while updating the expense.", response.ErrorMessage);
        }

        [Fact]
        public async Task DeleteExpense_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteExpenseAsync(userId, "User", expenseId))
                .ReturnsAsync(true);

            // Act
            var result = await controller.DeleteExpense(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task DeleteExpense_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteExpenseAsync(userId, "User", expenseId))
                .ReturnsAsync(false);

            // Act
            var result = await controller.DeleteExpense(expenseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Expense not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task DeleteExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteExpenseAsync(userId, "User", expenseId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.DeleteExpense(expenseId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while deleting the expense.", response.ErrorMessage);
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

        private static ExpenseController CreateController(Mock<IExpenseService> mockService)
        {
            return new ExpenseController(mockService.Object);
        }
    }
}
