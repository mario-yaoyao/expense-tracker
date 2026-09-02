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
    public class IncomeControllerTests
    {
        [Fact]
        public async Task GetIncomes_ReturnsOk_WhenIncomesExist()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var paginationReq = CreatePaginationRequest();
            var expectedResponseData = new List<IncomeResDto>
            {
                CreateIncomeResponse(id: 1, userId: userId, description: "Freelance", amount: 24.99m),
                CreateIncomeResponse(id: 2,  userId: userId, description: "Bonus", amount: 29.50m)
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
                .Setup(x => x.GetIncomesAsync(userId, "User", paginationReq))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetIncomes(paginationReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<IncomesResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(2, response.Data!.Metrics.TotalCount);
            Assert.Equal(expectedResponseData[0].Description, response.Data.Items[0].Description);
            Assert.Equal(expectedResponseData[1].Amount, response.Data.Items[1].Amount);
            Assert.Equal(expectedResponse.HighestAmount.Name, response.Data!.Metrics.HighestAmount!.Name);

            mockService.Verify(
                x => x.GetIncomesAsync(userId, "User", paginationReq),
                Times.Once);
        }

        [Fact]
        public async Task GetIncomes_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var paginationReq = CreatePaginationRequest();

            mockService.Setup(x => x.GetIncomesAsync(userId, "User", paginationReq))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetIncomes(paginationReq);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving incomes.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetIncomeById_ReturnsOk_WhenIncomeExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var expectedResponse = CreateIncomeResponse();

            mockService.Setup(x => x.GetIncomeByIdAsync(userId, "User", incomeId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetIncomeById(incomeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<IncomeResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task GetIncomeById_ReturnsNotFound_WhenNoIncomeExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.GetIncomeByIdAsync(userId, "User", incomeId))
                .ReturnsAsync((IncomeResDto?)null);

            // Act
            var result = await controller.GetIncomeById(incomeId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Income not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreateIncome_ReturnsOk_WhenIncomeCreated()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = CreateIncomeRequest(categoryId: categoryId);
            var expectedResponse = CreateIncomeResponse();

            mockService.Setup(x => x.CreateIncomeAsync(userId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.CreateIncome(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<IncomeResDto>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task CreateIncome_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = CreateIncomeRequest();

            mockService
                .Setup(x => x.CreateIncomeAsync(userId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.CreateIncome(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while creating the income.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateIncome_ReturnsOk_WhenIncomeExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = UpdateIncomeRequest();
            var expectedResponse = CreateIncomeResponse();

            mockService.Setup(x => x.UpdateIncomeAsync(userId, incomeId, request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.UpdateIncome(incomeId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<IncomeResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(expectedResponse.Description, response.Data!.Description);
        }

        [Fact]
        public async Task UpdateIncome_ReturnsNotFound_WhenNoIncomeExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = UpdateIncomeRequest();

            mockService.Setup(x => x.UpdateIncomeAsync(userId, incomeId, request))
                .ReturnsAsync((IncomeResDto?)null);

            // Act
            var result = await controller.UpdateIncome(incomeId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Income not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdateIncome_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            var request = UpdateIncomeRequest();

            mockService.Setup(x => x.UpdateIncomeAsync(userId, incomeId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.UpdateIncome(incomeId, request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while updating the income.", response.ErrorMessage);
        }

        [Fact]
        public async Task DeleteIncome_ReturnsOk_WhenIncomeExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteIncomeAsync(userId, incomeId))
                .ReturnsAsync(true);

            // Act
            var result = await controller.DeleteIncome(incomeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(okResult.Value);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task DeleteIncome_ReturnsNotFound_WhenNoIncomeExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteIncomeAsync(userId, incomeId))
                .ReturnsAsync(false);

            // Act
            var result = await controller.DeleteIncome(incomeId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<object>>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Income not found.", response.ErrorMessage);
        }

        [Fact]
        public async Task DeleteIncome_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockService = new Mock<IIncomeService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller, userId, "User");

            mockService.Setup(x => x.DeleteIncomeAsync(userId, incomeId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.DeleteIncome(incomeId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while deleting the income.", response.ErrorMessage);
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

        private static IncomeController CreateController(Mock<IIncomeService> mockService)
        {
            return new IncomeController(mockService.Object);
        }

        private static IncomeQueryReqDto CreatePaginationRequest(
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new IncomeQueryReqDto
            {
                Page = page,
                Limit = limit,
                Search = search
            };
        }

        private static IncomeResDto CreateIncomeResponse(
            int id = 1,
            int userId = 1,
            string description = "Monthly salary for month of august",
            decimal amount = 24.99m)
        {
            return new IncomeResDto
            {
                Id = id,
                UserId = userId,
                Description = description,
                Amount = amount,
                CategoryName = "Salary",
                CategoryType = CategoryType.Income,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static CreateIncomeReqDto CreateIncomeRequest(
            string description = "Freelance",
            decimal amount = 24.99m,
            int categoryId = 1)
        {
            return new CreateIncomeReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId
            };
        }

        private static UpdateIncomeReqDto UpdateIncomeRequest(
            string description = "Breakfast",
            decimal amount = 50.00m,
            int categoryId = 1)
        {
            return new UpdateIncomeReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId
            };
        }
    }
}
