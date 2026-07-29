using expense_tracker.Controllers;
using expense_tracker.Dtos.Requests;
using expense_tracker.Dtos.Responses;
using expense_tracker.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace expense_tracker.Tests.Controllers
{
    public class ExpensesControllerTests
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

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            Assert.IsType<List<ExpenseResDto>>(okResult.Value);
        }

        [Fact]
        public async Task GetExpenses_ReturnsNotFound_WhenNoExpensesExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync())
                .ReturnsAsync(new List<ExpenseResDto>());

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.GetExpenses();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetExpenses_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.GetExpensesAsync())
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.GetExpenses();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(500, statusCodeResult.StatusCode);
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

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.GetExpenseById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            Assert.IsType<ExpenseResDto>(okResult.Value);
        }

        [Fact]
        public async Task GetExpenseById_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var id = Guid.NewGuid();

            mockService.Setup(x => x.GetExpenseByIdAsync(id))
                .ReturnsAsync((ExpenseResDto?)null);

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.GetExpenseById(id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetExpenseById_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var id = Guid.NewGuid();

            mockService.Setup(x => x.GetExpenseByIdAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.GetExpenseById(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task CreateExpense_ReturnsOk_WhenExpenseCreated()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var expense = new ExpenseReqDto
            {
                Description = "Food"
            };

            mockService.Setup(x => x.CreateExpenseAsync(expense))
                .ReturnsAsync(new ExpenseResDto
                {
                    Id = Guid.NewGuid(),
                    Description = "Food"
                });

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.CreateExpense(expense);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
         
            Assert.IsType<ExpenseResDto>(okResult.Value);
        }

        [Fact]
        public async Task CreateExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();

            mockService.Setup(x => x.CreateExpenseAsync(It.IsAny<ExpenseReqDto>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.CreateExpense(new ExpenseReqDto());

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(500, objectResult.StatusCode);
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

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.UpdateExpense(id, expense);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            Assert.IsType<ExpenseResDto>(okResult.Value);
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

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.UpdateExpense(id, expense);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
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

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.UpdateExpense(id, expense);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task DeleteExpense_ReturnsOk_WhenExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var id = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(id))
                .ReturnsAsync(true);

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.DeleteExpense(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal("Expense deleted successfully.", okResult.Value);
        }

        [Fact]
        public async Task DeleteExpense_ReturnsNotFound_WhenNoExpenseExist()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var id = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(id))
                .ReturnsAsync(false);

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.DeleteExpense(id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteExpense_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var id = Guid.NewGuid();

            mockService.Setup(x => x.DeleteExpenseAsync(id))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new ExpensesController(mockService.Object);

            // Act
            var result = await controller.DeleteExpense(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
