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
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();

            var expectedResponse = new List<ExpenseResDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Description = "Breakfast",
                    Amount = 24.99m,
                    Category = "Food",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Description = "Lunch",
                    Amount = 29.50m,
                    Category = "Food",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                }
            };


            mockService
                .Setup(x => x.GetExpensesAsync(userId, "User"))
                .ReturnsAsync(expectedResponse);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<List<ExpenseResDto>>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal(2, response.data!.Count);
            Assert.Equal("Expenses retrieved successfully", response.message);
            Assert.Equal(expectedResponse[0].Description, response.data[0].Description);
            Assert.Equal(expectedResponse[1].Amount, response.data[1].Amount);

            mockService.Verify(
                x => x.GetExpensesAsync(userId, "User"),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenses_ReturnsNotFound_WhenNoExpensesExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();

            var expectedResponse = new List<ExpenseResDto>();

            mockService.Setup(x => x.GetExpensesAsync(userId, "User"))
                .ReturnsAsync(expectedResponse);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.success);
            Assert.Equal("No expenses found.", response.message);
        }

        [Fact]
        public async Task GetExpenses_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync(userId, "User"))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while retrieving expenses.", response.message);
        }

        [Fact]
        public async Task GetExpenseById_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var expectedResponse = new ExpenseResDto
            {
                Id = expenseId,
                UserId = userId,
                Description = "Food",
                Amount = 50.00m,
                Category = "Food",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockService.Setup(x => x.GetExpenseByIdAsync(userId, "User", expenseId))
                .ReturnsAsync(expectedResponse);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.GetExpenseById(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Expense retrieved successfully", response.message);
        }

        [Fact]
        public async Task GetExpenseById_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            mockService.Setup(x => x.GetExpenseByIdAsync(userId, "User", expenseId))
                .ReturnsAsync((ExpenseResDto?)null);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.GetExpenseById(expenseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.success);
            Assert.Equal("Expense not found.", response.message);
        }

        [Fact]
        public async Task CreateExpense_ReturnsOk_WhenExpenseCreated()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();

            var request = new CreateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                Category = "Food"
            };

            var expectedResponse = new ExpenseResDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Description = "Food"
            };

            mockService.Setup(x => x.CreateExpenseAsync(userId, request))
                .ReturnsAsync(expectedResponse);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.CreateExpense(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Expense record created successfully.", response.message);
        }

        [Fact]
        public async Task CreateExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();

            var request = new CreateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                Category = "Food"
            };

            mockService
                .Setup(x => x.CreateExpenseAsync(userId, request))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.CreateExpense(new CreateExpenseReqDto());
            SetUserClaims(controller, userId, "User");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while creating the expense.", response.message);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var request = new UpdateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                Category = "Food"
            };

            var expectedResponse = new ExpenseResDto
            {
                Id = expenseId,
                UserId = userId,
                Description = "Lunch",
                Amount = 50.00m,
                Category = "Food",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            mockService.Setup(x => x.UpdateExpenseAsync(userId, "User", expenseId, request))
                .ReturnsAsync(expectedResponse);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.UpdateExpense(expenseId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Expense record updated successfully.", response.message);
            Assert.Equal(expectedResponse.Description, response.data!.Description);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var request = new UpdateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                Category = "Food"
            };

            mockService.Setup(x => x.UpdateExpenseAsync(userId, "User", expenseId, request))
                .ReturnsAsync((ExpenseResDto?)null);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.UpdateExpense(expenseId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.success);
            Assert.Equal("Expense not found.", response.message);
        }

        [Fact]
        public async Task UpdateExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var request = new UpdateExpenseReqDto
            {
                Description = "Breakfast",
                Amount = 50.00m,
                Category = "Food"
            };

            mockService.Setup(x => x.UpdateExpenseAsync(userId, "User", expenseId, request))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.UpdateExpense(expenseId, request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while updating the expense.", response.message);
        }

        [Fact]
        public async Task DeleteExpense_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(userId, "User", expenseId))
                .ReturnsAsync(true);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.DeleteExpense(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Expense record deleted successfully.", response.message);
        }

        [Fact]
        public async Task DeleteExpense_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(userId, "User", expenseId))
                .ReturnsAsync(false);

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.DeleteExpense(expenseId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.success);
            Assert.Equal("Expense not found.", response.message);
        }

        [Fact]
        public async Task DeleteExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(userId, "User", expenseId))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.DeleteExpense(expenseId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while deleting the expense.", response.message);
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
