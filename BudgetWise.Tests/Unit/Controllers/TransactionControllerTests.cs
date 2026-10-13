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
    public class TransactionControllerTests
    {
        [Fact]
        public async Task GetTransactions_ReturnsOk_WhenTransactionsExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ITransactionService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var queryReq = CreateQueryRequest();

            var mappedTransactions = new List<TransactionResDto>
            {
                CreateTransactionResponse(),
                CreateTransactionResponse(type: TransactionType.Update)
            };

            var expectedResponse = (
                Data: mappedTransactions,
                HasNextPage: false
            );

            mockService
                .Setup(x => x.GetTransactionsAsync(userId, "User", queryReq))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetTransactions(queryReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TransactionsResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(mappedTransactions[0].Username, response.Data!.Items[0].Username);
            Assert.Equal(mappedTransactions[1].Type, response.Data!.Items[1].Type);

            mockService.Verify(
                x => x.GetTransactionsAsync(userId, "User", queryReq),
                Times.Once);
        }

        [Fact]
        public async Task GetTransactions_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<ITransactionService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var queryReq = CreateQueryRequest();

            mockService.Setup(x => x.GetTransactionsAsync(userId, "User", queryReq))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetTransactions(queryReq);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving transactions.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetTransactionById_ReturnsOk_WhenTransactionExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ITransactionService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var expectedResponse = CreateTransactionResponse();

            mockService.Setup(x => x.GetTransactionByIdAsync(userId, "User", categoryId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetTransactionById(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<TransactionResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task GetTransactionById_ReturnsNotFound_WhenNoTransactionExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<ITransactionService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            mockService.Setup(x => x.GetTransactionByIdAsync(userId, "User", categoryId))
                .ReturnsAsync((TransactionResDto?)null);

            // Act
            var result = await controller.GetTransactionById(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Transaction not found.", response.ErrorMessage);
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

        private static TransactionController CreateController(Mock<ITransactionService> mockService)
        {
            return new TransactionController(mockService.Object);
        }

        private static TransactionResDto CreateTransactionResponse(
            int id = 1,
            string message = "Created category",
            DateTime? timestamp = null,
            int? userId = 1,
            string? username = "testuser",
            TransactionType? type = TransactionType.Create,
            string? activity = "Created category")
        {
            return new TransactionResDto
            {
                Id = id,
                Message = message,
                TimeStamp = timestamp ?? DateTime.UtcNow,
                UserId = userId,
                Username = username,
                Type = type,
                Activity = activity
            };
        }

        private static TransactionQueryReqDto CreateQueryRequest(
            TransactionType type = TransactionType.Create,
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new TransactionQueryReqDto
            {
                Type = type,
                Page = page,
                Limit = limit,
                Search = search
            };
        }
    }
}
