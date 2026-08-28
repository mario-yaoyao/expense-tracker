using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Controllers;
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

            var paginationReq = CreatePaginationRequest();
            var expectedResponseData = new List<ExpenseResDto>
            {
                CreateExpenseResponse(id: 1, userId: userId, description: "Breakfast", amount: 24.99m),
                CreateExpenseResponse(id: 2,  userId: userId, description: "Lunch", amount: 29.50m)
            };

            var expectedResponse = (
                Data: expectedResponseData,
                TotalExpense: 49.49m,
                HighestAmount: new HighestAmountResDto
                {
                    Name = expectedResponseData[1].Description,
                    Amount = expectedResponseData[1].Amount,
                },
                TotalCount: 2,
                HasNextPage: false
            );

            mockService
                .Setup(x => x.GetExpensesAsync(userId, "User", paginationReq))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetExpenses(paginationReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<ExpensesResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(2, response.Data!.Metrics.TotalCount);
            Assert.Equal(expectedResponseData[0].Description, response.Data.Items[0].Description);
            Assert.Equal(expectedResponseData[1].Amount, response.Data.Items[1].Amount);
            Assert.Equal(expectedResponse.HighestAmount.Name, response.Data!.Metrics.HighestAmount!.Name);

            mockService.Verify(
                x => x.GetExpensesAsync(userId, "User", paginationReq),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenses_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IExpenseService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var paginationReq = CreatePaginationRequest();

            mockService.Setup(x => x.GetExpensesAsync(userId, "User", paginationReq))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetExpenses(paginationReq);

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

            var expectedResponse = CreateExpenseResponse();

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

            var request = CreateExpenseRequest(categoryId: categoryId);
            var expectedResponse = CreateExpenseResponse();

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

            var request = CreateExpenseRequest();

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

            var request = UpdateExpenseRequest();
            var expectedResponse = CreateExpenseResponse();

            mockService.Setup(x => x.UpdateExpenseAsync(userId, expenseId, request))
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

            var request = UpdateExpenseRequest();

            mockService.Setup(x => x.UpdateExpenseAsync(userId, expenseId, request))
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

            var request = UpdateExpenseRequest();

            mockService.Setup(x => x.UpdateExpenseAsync(userId, expenseId, request))
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

            mockService.Setup(x => x.DeleteExpenseAsync(userId, expenseId))
                .ReturnsAsync(true);

            // Act
            var result = await controller.DeleteExpense(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

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

            mockService.Setup(x => x.DeleteExpenseAsync(userId, expenseId))
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

            mockService.Setup(x => x.DeleteExpenseAsync(userId, expenseId))
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

        private static ExpenseQueryReqDto CreatePaginationRequest(
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new ExpenseQueryReqDto
            {
                Page = page,
                Limit = limit,
                Search = search
            };
        }

        private static ExpenseResDto CreateExpenseResponse(
            int id = 1,
            int userId = 1,
            string description = "Breakfast",
            decimal amount = 24.99m)
        {
            return new ExpenseResDto
            {
                Id = id,
                UserId = userId,
                Description = description,
                Amount = amount,
                CategoryName = "Food",
                CategoryType = 0,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static CreateExpenseReqDto CreateExpenseRequest(
            string description = "Breakfast",
            decimal amount = 50.00m,
            int categoryId = 1)
        {
            return new CreateExpenseReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId
            };
        }

        private static UpdateExpenseReqDto UpdateExpenseRequest(
            string description = "Breakfast",
            decimal amount = 50.00m,
            int categoryId = 1)
        {
            return new UpdateExpenseReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId
            };
        }
    }
}
