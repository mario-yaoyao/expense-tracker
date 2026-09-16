using AutoMapper;
using BudgetWise.BLL.Services;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Moq;

namespace BudgetWise.Tests.Unit.Services
{
    public class DashboardServiceTests
    {
        [Fact]
        public async Task GetDashboardAsync_ReturnsGeneralDetails_WhenRoleIsSuperAdmin()
        {
            // Arrange
            var secondUserId = 2;
            var thirdUserId = 1;

            var mockRepo = new Mock<IDashboardRepository>();
            var service = new DashboardService(mockRepo.Object, mockMapper.Object);

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

            var recentUsersData = new List<RecentUsersResDto>
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
                users,
                recentTransactions
            );

            mockRepo
                .Setup(x => x.GetSuperAdminDashboardAsync())
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<RecentUsersResDto>>(It.IsAny<List<User>>()))
                .Returns(recentUsersData);

            // Act
            var result = await service.GetSuperAdminDashboardAsync();

            // Assert
            Assert.Equal(metrics.NewUsers, result.metrics.NewUsers);
            Assert.Equal(userGrowthTrend[6].NewUsers, result.usersGrowthTrend[6].NewUsers);
            Assert.Equal(users[1].Username, result.recentUsers[1].Username);
            Assert.Equal(users[2].Role, result.recentUsers[2].Role);
            Assert.Equal(recentTransactions[0].Username, result.recentTransactions[0].Username);

            mockRepo.Verify(
                x => x.GetSuperAdminDashboardAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetDashboardAsync_ReturnsOwnDetails_WhenRoleIsUser()
        {
            // Arrange
            var userId = 1;

            var mockRepo = new Mock<IDashboardRepository>();
            var service = new DashboardService(mockRepo.Object, mockMapper.Object);

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

            mockRepo
                .Setup(x => x.GetUserDashboardAsync(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await service.GetUserDashboardAsync(userId);

            // Assert
            Assert.Equal(metrics.Balance, result.metrics.Balance);
            Assert.Equal(savingsTrend[9].Savings, result.savingsTrend[9].Savings);
            Assert.Equal(incomeExpenseTrend[1].Income, result.incomeExpenseTrend[1].Income);
            Assert.Equal(recentTransactions[0].Username, result.recentTransactions[0].Username);

            mockRepo.Verify(
                x => x.GetUserDashboardAsync(userId),
                Times.Once);
        }

        // Helper Functions
        private readonly Mock<IMapper> mockMapper;

        public DashboardServiceTests()
        {
            mockMapper = new Mock<IMapper>();
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
