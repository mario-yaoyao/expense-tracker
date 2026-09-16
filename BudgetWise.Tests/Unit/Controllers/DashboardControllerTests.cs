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
    public class DashboardControllerTests
    {
        [Fact]
        public async Task GetSuperAdminDashboard_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var secondUserId = 2;
            var thirdUserId = 3;

            var mockService = new Mock<IDashboardService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            var users = new List<User>
            {
                CreateUser(),
                CreateUser(id: secondUserId, username: "testusertwo", isActive: false),
                CreateUser(id: thirdUserId, username: "testuserthree", createdAt: new DateTime(2026, 7, 15))
            };

            var metrics = new SuperAdminDashboardMetricsResDto
            {
                TotalUsers = 3,
                ActiveUsers = 2,
                NewUsers = 2
            };

            var userGrowthTrend = new List<UserGrowthTrendResDto>
            {
                new() { Month = "Jan", NewUsers = 0 },
                new() { Month = "Feb", NewUsers = 0 },
                new() { Month = "Mar", NewUsers = 0 },
                new() { Month = "Apr", NewUsers = 0 },
                new() { Month = "May", NewUsers = 0 },
                new() { Month = "Jun", NewUsers = 0 },
                new() { Month = "Jul", NewUsers = 1 },
                new() { Month = "Aug", NewUsers = 0 },
                new() { Month = "Sep", NewUsers = 9 },
                new() { Month = "Oct", NewUsers = 0 },
                new() { Month = "Nov", NewUsers = 0 },
                new() { Month = "Dec", NewUsers = 0 }
            };

            var recentUsers = new List<RecentUsersResDto>
            {
                new()
                {
                    Id = users[0].Id,
                    Username = users[0].Username,
                    Role = users[0].Role,
                    CreatedAt = users[0].CreatedAt
                },
                new()
                {
                    Id = users[1].Id,
                    Username = users[1].Username,
                    Role = users[1].Role,
                    CreatedAt = users[1].CreatedAt
                },
                new()
                {
                    Id = users[2].Id,
                    Username = users[2].Username,
                    Role = users[2].Role,
                    CreatedAt = users[2].CreatedAt
                }
            };      

            var description = "test description";
            var recentTransactions = new List<RecentTransactionsResDto>
            {
                new()
                {
                    Id = 1,
                    UserId = 1,
                    Username = users[0].Username,
                    Action = "Create",
                    Activity = $"Created expense '{description}'",
                    Message = $"'{users[0].Username}' created expense '{description}'.",
                    CreatedAt = new DateTime(2026, 7, 15)
                }
            };

            var expectedResponse = (
                metrics,
                userGrowthTrend,
                recentUsers,
                recentTransactions
            );

            mockService
                .Setup(x => x.GetSuperAdminDashboardAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetSuperAdminDashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<SuperAdminDashboardResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(3, response.Data.Metrics.TotalUsers);
            Assert.Equal(2, response.Data.Metrics.ActiveUsers);
            Assert.Equal(2, response.Data.Metrics.NewUsers);
            Assert.Single(response.Data.RecentTransactions);
            Assert.Equal(users[0].Username, response.Data.RecentTransactions[0].Username);
            Assert.Equal(3, response.Data.RecentUsers.Count);
            Assert.Equal(12, response.Data.UsersGrowthTrend.Count);

            mockService.Verify(
                x => x.GetSuperAdminDashboardAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetSuperAdminDashboard_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var mockService = new Mock<IDashboardService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller);

            mockService
                .Setup(x => x.GetSuperAdminDashboardAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetSuperAdminDashboard();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving dashboard record.", response.ErrorMessage);
        }

        [Fact]
        public async Task GetUserDashboard_ReturnsOk_WhenDataExists()
        {
            // Arrange
            var firstUserId = 1;

            var mockService = new Mock<IDashboardService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller: controller, role: "User");

            var user = CreateUser();

            var incomes = new List<IncomeResDto>
            {
                CreateIncome(amount: 1000),
                CreateIncome(id: 2, amount: 1500)
            };

            var expenses = new List<CreateExpenseReqDto>
            {
                CreateExpense(amount: 400),
                CreateExpense(description: "Lunch", amount: 600)
            };

            var totalIncome = incomes.Sum(i => i.Amount);
            var totalExpense = expenses.Sum(e => e.Amount);

            var metrics = new UserDashboardMetricsResDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome - totalExpense
            };

            var savingsTrend = new List<SavingsTrendResDto>
            {
                new() { Month = "Jan", Savings = 0 },
                new() { Month = "Feb", Savings = 0 },
                new() { Month = "Mar", Savings = 0 },
                new() { Month = "Apr", Savings = 0 },
                new() { Month = "May", Savings = 0 },
                new() { Month = "Jun", Savings = 0 },
                new() { Month = "Jul", Savings = 1 },
                new() { Month = "Aug", Savings = 0 },
                new() { Month = "Sep", Savings = 9 },
                new() { Month = "Oct", Savings = 0 },
                new() { Month = "Nov", Savings = 0 },
                new() { Month = "Dec", Savings = 0 }
            };

            var incomeExpenseTrend = new List<IncomeExpenseTrendResDto>
            {
                new()
                {
                    Month = "Aug",
                    Income = 1000,
                    Expense = 400
                },
                new()
                {
                    Month = "Sep",
                    Income = 1500,
                    Expense = 600
                }
            };

            var description = "test description";
            var recentTransactions = new List<RecentTransactionsResDto>
            {
                new()
                {
                    Id = 1,
                    UserId = 1,
                    Username = user.Username,
                    Action = "Create",
                    Activity = $"Created expense '{description}'",
                    Message = $"'{user.Username}' created expense '{description}'.",
                    CreatedAt = new DateTime(2026, 7, 15)
                },
                new()
                {
                    Id = 2,
                    UserId = 1,
                    Username = user.Username,
                    Action = "Update",
                    Activity = $"Updated income '{description}'",
                    Message = $"'{user.Username}' updated income '{description}'.",
                    CreatedAt = new DateTime(2026, 7, 15)
                }
            };

            var expectedResponse = (
                metrics,
                savingsTrend,
                incomeExpenseTrend,
                recentTransactions
            );

            mockService
                .Setup(x => x.GetUserDashboardAsync(firstUserId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await controller.GetUserDashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResDto<UserDashboardResDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(totalIncome, response.Data.Metrics.TotalIncome);
            Assert.Equal(totalExpense, response.Data.Metrics.TotalExpense);
            Assert.Equal(totalIncome - totalExpense, response.Data.Metrics.Balance);
            Assert.Equal(2, response.Data.RecentTransactions.Count);
            Assert.Equal(user.Username, response.Data.RecentTransactions[0].Username);
            Assert.Equal(12, response.Data.SavingsTrend.Count);
            Assert.Equal(2, response.Data.IncomeExpenseTrend.Count);
            Assert.Equal(2, response.Data.RecentTransactions.Count);

            mockService.Verify(
                x => x.GetUserDashboardAsync(firstUserId),
                Times.Once);
        }

        [Fact]
        public async Task GetUserDashboard_Returns500_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;

            var mockService = new Mock<IDashboardService>();
            var controller = CreateController(mockService);
            SetUserClaims(controller: controller, role: "User");

            mockService
                .Setup(x => x.GetUserDashboardAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await controller.GetUserDashboard();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = Assert.IsType<ApiResDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("An error occurred while retrieving dashboard record.", response.ErrorMessage);
        }

        // Helper Functions
        private static void SetUserClaims(ControllerBase controller, int userId = 1, string role = "SuperAdmin")
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

        private static DashboardController CreateController(Mock<IDashboardService> mockService)
        {
            return new DashboardController(mockService.Object);
        }

        private static User CreateUser(
            int id = 1,
            string username = "testuser",
            bool isActive = true,
            DateTime? createdAt = null)
        {
            return new User
            {
                Id = id,
                FullName = "Test User",
                Username = username,
                ContactNumber = "09123456789",
                HashedPassword = "password",
                Role = UserRole.User,
                IsActive = isActive,
                CreatedAt = createdAt ?? DateTime.Now,
            };
        }

        private static CreateExpenseReqDto CreateExpense(
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

        private static IncomeResDto CreateIncome(
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
    }
}
