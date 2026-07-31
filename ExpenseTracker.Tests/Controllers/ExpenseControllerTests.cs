using ExpenseTracker.Controllers;
using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace ExpenseTracker.Tests.Controllers
{
    public class ExpenseControllerTests
    {
        [Fact]
        public async Task GetExpenses_ReturnsOk_WhenExpensesExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync())
                .ReturnsAsync(new List<ExpenseResDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Description = "Food"
                    }
                });

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<List<ExpenseResDto>>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Expenses retrieved successfully", response.message);
        }

        [Fact]
        public async Task GetExpenses_ReturnsNotFound_WhenNoExpensesExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync())
                .ReturnsAsync(new List<ExpenseResDto>());

            var controller = new ExpenseController(mockService.Object);

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
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync())
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);

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
            var id = Guid.NewGuid();

            mockService.Setup(x => x.GetExpenseByIdAsync(id))
                .ReturnsAsync(new ExpenseResDto
                {
                    Id = id,
                    Description = "Food"
                });

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.GetExpenseById(id);

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
            var id = Guid.NewGuid();

            mockService.Setup(x => x.GetExpenseByIdAsync(id))
                .ReturnsAsync((ExpenseResDto?)null);

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.GetExpenseById(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.success);
            Assert.Equal("Expense not found.", response.message);
        }

        [Fact]
        public async Task GetExpenseById_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var id = Guid.NewGuid();

            mockService.Setup(x => x.GetExpenseByIdAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.GetExpenseById(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while retrieving the expense.", response.message);
        }

        [Fact]
        public async Task CreateExpense_ReturnsOk_WhenExpenseCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            var expense = new ExpenseReqDto
            {
                Description = "Food"
            };

            var expectedResponse = new ExpenseResDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Description = "Food"
            };

            mockService.Setup(x => x.CreateExpenseAsync(userId, expense))
                .ReturnsAsync(expectedResponse);

            var controller = new ExpenseController(mockService.Object);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            // Act
            var result = await controller.CreateExpense(expense);

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
            var userId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            mockService
                .Setup(x => x.CreateExpenseAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ExpenseReqDto>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "TestAuth"))
                }
            };

            // Act
            var result = await controller.CreateExpense(new ExpenseReqDto());

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
            var id = Guid.NewGuid();

            var expense = new ExpenseReqDto
            {
                Description = "Food"
            };

            mockService.Setup(x => x.UpdateExpenseAsync(id, expense))
                .ReturnsAsync(new ExpenseResDto
                {
                    Id = id,
                    Description = "Food"
                });

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.UpdateExpense(id, expense);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Expense record updated successfully.", response.message);
        }

        [Fact]
        public async Task UpdateExpense_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var id = Guid.NewGuid();

            var expense = new ExpenseReqDto
            {
                Description = "Food"
            };

            mockService.Setup(x => x.UpdateExpenseAsync(id, expense))
                .ReturnsAsync((ExpenseResDto?)null);

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.UpdateExpense(id, expense);

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
            var id = Guid.NewGuid();

            var expense = new ExpenseReqDto
            {
                Description = "Food"
            };

            mockService.Setup(x => x.UpdateExpenseAsync(id, expense))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.UpdateExpense(id, expense);

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
            var id = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(id))
                .ReturnsAsync(true);

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.DeleteExpense(id);

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
            var id = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(id))
                .ReturnsAsync(false);

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.DeleteExpense(id);

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
            var id = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpenseController(mockService.Object);

            // Act
            var result = await controller.DeleteExpense(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.success);
            Assert.Contains("An error occurred while deleting the expense.", response.message);
        }
    }
}
