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
            var userId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync(userId, "User"))
                .ReturnsAsync(new List<ExpenseResDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Description = "Food"
                    }
                });

            var controller = new ExpenseController(mockService.Object);
            SetUserClaims(controller, userId, "User");

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<List<ExpenseResDto>>>(okResult.Value);

            Assert.True(response.success);
            Assert.Equal("Expenses retrieved successfully", response.message);

            mockService.Verify(
                x => x.GetExpensesAsync(userId, "User"),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenses_ReturnsNotFound_WhenNoExpensesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync(userId, "User"))
                .ReturnsAsync(new List<ExpenseResDto>());

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

        //[Fact]
        //public async Task GetExpenseById_ReturnsOk_WhenExpenseExist()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var expenseId = Guid.NewGuid();
        //    var mockService = new Mock<IExpenseService>();

        //    mockService.Setup(x => x.GetExpenseByIdAsync(userId, expenseId))
        //        .ReturnsAsync(new ExpenseResDto
        //        {
        //            Id = expenseId,
        //            Description = "Food"
        //        });

        //    var controller = new ExpenseController(mockService.Object);
        //    SetUserClaims(controller, userId, "User");

        //    // Act
        //    var result = await controller.GetExpenseById(expenseId);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

        //    Assert.True(response.success);
        //    Assert.Equal("Expense retrieved successfully", response.message);
        //}

        //[Fact]
        //public async Task GetExpenseById_ReturnsNotFound_WhenNoExpenseExist()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var expenseId = Guid.NewGuid();
        //    var mockService = new Mock<IExpenseService>();

        //    mockService.Setup(x => x.GetExpenseByIdAsync(userId, expenseId))
        //        .ReturnsAsync((ExpenseResDto?)null);

        //    var controller = new ExpenseController(mockService.Object);
        //    SetUserClaims(controller, userId, "User");

        //    // Act
        //    var result = await controller.GetExpenseById(expenseId);

        //    // Assert
        //    var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

        //    Assert.False(response.success);
        //    Assert.Equal("Expense not found.", response.message);
        //}

        //[Fact]s

        //[Fact]
        //public async Task CreateExpense_ReturnsOk_WhenExpenseCreated()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var mockService = new Mock<IExpenseService>();

        //    var expense = new ExpenseReqDto
        //    {
        //        Description = "Food"
        //    };

        //    var expectedResponse = new ExpenseResDto
        //    {
        //        Id = Guid.NewGuid(),
        //        UserId = userId,
        //        Description = "Food"
        //    };

        //    mockService.Setup(x => x.CreateExpenseAsync(userId, expense))
        //        .ReturnsAsync(expectedResponse);

        //    var controller = new ExpenseController(mockService.Object);
        //    SetUserClaims(controller, userId, "User");

        //    // Act
        //    var result = await controller.CreateExpense(expense);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

        //    Assert.True(response.success);
        //    Assert.Equal("Expense record created successfully.", response.message);
        //}

        //[Fact]
        //public async Task CreateExpense_Returns500_WhenExceptionOccurs()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var mockService = new Mock<IExpenseService>();

        //    mockService
        //        .Setup(x => x.CreateExpenseAsync(
        //            userId,
        //            It.IsAny<ExpenseReqDto>()))
        //        .ThrowsAsync(new Exception("Database error"));

        //    var controller = new ExpenseController(mockService.Object);
        //    SetUserClaims(controller, userId, "User");

        //    // Act
        //    var result = await controller.CreateExpense(new ExpenseReqDto());

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        //    Assert.Equal(500, statusCodeResult.StatusCode);

        //    var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
        //    Assert.False(response.success);
        //    Assert.Contains("An error occurred while creating the expense.", response.message);
        //}

        //[Fact]
        //public async Task UpdateExpense_ReturnsOk_WhenExpenseExist()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var expenseId = Guid.NewGuid();
        //    var mockService = new Mock<IExpenseService>();

        //    var expense = new ExpenseReqDto
        //    {
        //        Description = "Food"
        //    };

        //    mockService.Setup(x => x.UpdateExpenseAsync(userId, expenseId, expense))
        //        .ReturnsAsync(new ExpenseResDto
        //        {
        //            Id = expenseId,
        //            Description = "Food"
        //        });

        //    var controller = new ExpenseController(mockService.Object);
        //    SetUserClaims(controller, userId, "User");

        //    // Act
        //    var result = await controller.UpdateExpense(expenseId, expense);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<ExpenseResDto>>(okResult.Value);

        //    Assert.True(response.success);
        //    Assert.Equal("Expense record updated successfully.", response.message);
        //}

        //[Fact]
        //public async Task UpdateExpense_ReturnsNotFound_WhenNoExpenseExist()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var expenseId = Guid.NewGuid();
        //    var mockService = new Mock<IExpenseService>();

        //    var expense = new ExpenseReqDto
        //    {
        //        Description = "Food"
        //    };

        //    mockService.Setup(x => x.UpdateExpenseAsync(userId, expenseId, expense))
        //        .ReturnsAsync((ExpenseResDto?)null);

        //    var controller = new ExpenseController(mockService.Object);
        //    SetUserClaims(controller, userId, "User");

        //    // Act
        //    var result = await controller.UpdateExpense(expenseId, expense);

        //    // Assert
        //    var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        //    var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

        //    Assert.False(response.success);
        //    Assert.Equal("Expense not found.", response.message);
        //}

        //[Fact]
        //public async Task UpdateExpense_Returns500_WhenExceptionOccurs()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var expenseId = Guid.NewGuid();
        //    var mockService = new Mock<IExpenseService>();

        //    var expense = new ExpenseReqDto
        //    {
        //        Description = "Food"
        //    };

        //    mockService.Setup(x => x.UpdateExpenseAsync(userId, expenseId, expense))
        //        .ThrowsAsync(new Exception("Database error"));

        //    var controller = new ExpenseController(mockService.Object);
        //    SetUserClaims(controller, userId, "User");

        //    // Act
        //    var result = await controller.UpdateExpense(expenseId, expense);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        //    Assert.Equal(500, statusCodeResult.StatusCode);

        //    var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
        //    Assert.False(response.success);
        //    Assert.Contains("An error occurred while updating the expense.", response.message);
        //}

        [Fact]
        public async Task DeleteExpense_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.DeleteExpenseAsync(userId, expenseId))
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
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.DeleteExpenseAsync(userId, expenseId))
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
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.DeleteExpenseAsync(userId, expenseId))
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

        private static void SetUserClaims(
            ControllerBase controller,
            Guid userId,
            string role)
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
