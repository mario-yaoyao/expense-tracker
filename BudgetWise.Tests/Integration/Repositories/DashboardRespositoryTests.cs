using BudgetWise.DAL.Data;
using BudgetWise.DAL.Repositories;
using BudgetWise.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetWise.Tests.Integration.Repositories;

public class DashboardRepositoryTests
{
    [Fact]
    public async Task GetSuperAdminDashboardAsync_ReturnsCorrectMetrics()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        var users = new List<User>
        {
            CreateUser(),
            CreateUser(id: 2),
            CreateUser(id: 3, createdAt: new DateTime(2026, 7, 15)),
            CreateUser(id: 4, isActive: false),
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Act
        var (metrics, _, _, _) = await repository.GetSuperAdminDashboardAsync();

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(4, metrics.TotalUsers);
        Assert.Equal(3, metrics.ActiveUsers);
        Assert.Equal(3, metrics.NewUsers);
    }

    [Fact]
    public async Task GetSuperAdminDashboardAsync_ReturnsCorrectUserGrowthTrend()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        var users = new List<User>
        {
            CreateUser(createdAt: new DateTime(2026, 8, 10)),
            CreateUser(id: 2, createdAt: new DateTime(2026, 8, 15)),
            CreateUser(id: 3, isActive: false, createdAt: new DateTime(2026, 8, 20)),
            CreateUser(id: 4, createdAt: new DateTime(2026, 9, 1))
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Act
        var (_, userGrowthTrend, _, _) = await repository.GetSuperAdminDashboardAsync();

        // Assert
        Assert.NotNull(userGrowthTrend);

        var august = userGrowthTrend.First(x => x.Month == "Aug");
        var september = userGrowthTrend.First(x => x.Month == "Sep");

        Assert.Equal(3, august.NewUsers);
        Assert.Equal(1, september.NewUsers);
    }

    [Fact]
    public async Task GetSuperAdminDashboardAsync_ReturnsMaximumTenRecentUsers()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        var users = Enumerable.Range(1, 12)
            .Select(id =>CreateUser(id: id, createdAt: DateTime.UtcNow.AddDays(-id)))
            .ToList();

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Act
        var (_, _, recentUsers, _) = await repository.GetSuperAdminDashboardAsync();

        // Assert
        Assert.NotNull(recentUsers);
        Assert.Equal(10, recentUsers.Count);
    }

    [Fact]
    public async Task GetSuperAdminDashboardAsync_ReturnsRecentTransactions()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        var user = CreateUser();
        context.Users.Add(user);

        var transactionLogs = Enumerable.Range(1, 11)
            .Select(i => new Transaction
            {
                Id = i,
                UserId = user.Id,
                Username = user.Username,
                Type = i % 2 == 0 ? TransactionType.Update : TransactionType.Create,
                Message = $"{user.Username} performed transaction {i}",
                TimeStamp = new DateTime(2026, 9, 1).AddDays(i)
            })
            .ToList();

        context.Transactions.AddRange(transactionLogs);
        await context.SaveChangesAsync();

        // Act
        var (_, _, _, recentTransactions) = await repository.GetSuperAdminDashboardAsync();

        // Assert
        Assert.NotNull(recentTransactions);
        Assert.Equal(10, recentTransactions.Count);
        Assert.Equal("testuser performed transaction 2", recentTransactions[9].Message);
    }

    [Fact]
    public async Task GetSuperAdminDashboardAsync_ReturnsRecentUsersOrderedDescending()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        var users = new List<User>
        {
            CreateUser(createdAt: DateTime.UtcNow.AddDays(-3)),
            CreateUser(id: 2, createdAt: DateTime.UtcNow.AddDays(-2)),
            CreateUser(id: 3, createdAt: DateTime.UtcNow.AddDays(-1))
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        await context.SaveChangesAsync();

        // Act
        var (_, _, recentUsers, _) = await repository.GetSuperAdminDashboardAsync();

        // Assert
        Assert.Equal(3, recentUsers[0].Id);
        Assert.Equal(2, recentUsers[1].Id);
        Assert.Equal(1, recentUsers[2].Id);
    }

    [Fact]
    public async Task GetSuperAdminDashboardAsync_ReturnsEmptyDashboard_WhenNoUsersExist()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        // Act
        var (metrics, userGrowthTrend, recentUsers, recentTransactions) = await repository.GetSuperAdminDashboardAsync();

        // Assert
        Assert.Equal(0, metrics.TotalUsers);
        Assert.Equal(0, metrics.ActiveUsers);
        Assert.Equal(0, metrics.NewUsers);
        Assert.Equal(12, userGrowthTrend.Count);

        Assert.All(
            userGrowthTrend,
            item => Assert.Equal(0, item.NewUsers));

        Assert.Empty(recentUsers);
    }

    [Fact]
    public async Task GetUserDashboardAsync_ReturnsCorrectMetrics()
    {
        // Arrange
        var userId = 1;

        using var context = CreateContext();
        var repository = CreateRepository(context);

        var incomes = new List<Income>
        {
            CreateIncome(amount: 1000),
            CreateIncome(id: 2, amount: 1500)
        };

        var expenses = new List<Expense>
        {
            CreateExpense(amount: 400),
            CreateExpense(id: 2, description: "Lunch", amount: 600)
        };

        context.Incomes.AddRange(incomes);
        context.Expenses.AddRange(expenses);
        await context.SaveChangesAsync();

        // Act
        var (metrics, _, _, _) = await repository.GetUserDashboardAsync(userId);

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(2500, metrics.TotalIncome);
        Assert.Equal(1000, metrics.TotalExpense);
        Assert.Equal(1500, metrics.Balance);
    }

    [Fact]
    public async Task GetUserDashboardAsync_ReturnsCorrectSavingsTrend()
    {
        // Arrange
        var userId = 1;

        using var context = CreateContext();
        var repository = CreateRepository(context);

        var incomes = new List<Income>
        {
            CreateIncome(amount: 1000),
            CreateIncome(id: 2, amount: 1500, createdAt: new DateTime(2026, 8, 15))
        };

        var expenses = new List<Expense>
        {
            CreateExpense(amount: 400),
            CreateExpense(id: 2, description: "Lunch", amount: 600, createdAt: new DateTime(2026, 8, 15))
        };

        context.Incomes.AddRange(incomes);
        context.Expenses.AddRange(expenses);
        await context.SaveChangesAsync();

        // Act
        var (_, savingsTrend, _, _) = await repository.GetUserDashboardAsync(userId);

        // Assert
        Assert.NotNull(savingsTrend);

        var august = savingsTrend.First(x => x.Month == "Aug");
        var september = savingsTrend.First(x => x.Month == "Sep");

        Assert.Equal(900, august.Savings);
        Assert.Equal(600, september.Savings);
    }

    [Fact]
    public async Task GetUserDashboardAsync_ReturnsCorrectIncomeExpenseTrend()
    {
        // Arrange
        var userId = 1;

        using var context = CreateContext();
        var repository = CreateRepository(context);

        var incomes = new List<Income>
        {
            CreateIncome(amount: 1000),
            CreateIncome(id: 2, amount: 1500, createdAt: new DateTime(2026, 8, 15))
        };

        var expenses = new List<Expense>
        {
            CreateExpense(amount: 400),
            CreateExpense(id: 2, description: "Lunch", amount: 600, createdAt: new DateTime(2026, 8, 15))
        };

        context.Incomes.AddRange(incomes);
        context.Expenses.AddRange(expenses);
        await context.SaveChangesAsync();

        // Act
        var (_, _, incomeExpenseTrend, _) = await repository.GetUserDashboardAsync(userId);

        // Assert
        Assert.NotNull(incomeExpenseTrend);
        Assert.Equal(incomes[1].Amount, incomeExpenseTrend[0].Income);
        Assert.Equal(expenses[0].Amount, incomeExpenseTrend[1].Expense);
        Assert.Equal(2, incomeExpenseTrend.Count);
    }

    [Fact]
    public async Task GetUserDashboardAsync_ReturnsRecentTransactions()
    {
        // Arrange
        using var context = CreateContext();
        var repository = CreateRepository(context);

        var user = CreateUser();

        var transactionLogs = Enumerable.Range(1, 11)
            .Select(i => new Transaction
            {
                Id = i,
                UserId = user.Id,
                Username = user.Username,
                Type = i % 2 == 0 ? TransactionType.Update : TransactionType.Create,
                Activity = i % 2 == 0
                    ? $"Updated transaction {i}"
                    : $"Created transaction {i}",
                TimeStamp = new DateTime(2026, 9, 1).AddDays(i)
            })
            .ToList();

        context.Users.Add(user);
        context.Transactions.AddRange(transactionLogs);
        await context.SaveChangesAsync();

        // Act
        var (_, _, _, recentTransactions) = await repository.GetUserDashboardAsync(user.Id);

        // Assert
        Assert.NotNull(recentTransactions);
        Assert.Equal(10, recentTransactions.Count);

        Assert.Contains(
            recentTransactions,
            t => t.Activity == "Updated transaction 2");

        Assert.Contains(
            recentTransactions,
            t => t.Activity == "Created transaction 11");
    }

    // Helper Functions
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static DashboardRepository CreateRepository(
        AppDbContext context)
    {
        var mockLogger = new Mock<ILogger<DashboardRepository>>();

        return new DashboardRepository(context, mockLogger.Object);
    }

    private static User CreateUser(
        int id = 1,
        string username = "testuser",
        UserRole role = UserRole.User,
        bool isActive = true,
        DateTime? createdAt = null)
    {
        return new User
        {
            Id = id,
            Username = username,
            FullName = "Test User",
            Email = $"{username}@gmail.com",
            ContactNumber = "09123456789",
            HashedPassword = "password",
            Role = role,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }

    private static Expense CreateExpense(
        int id = 1,
        int userId = 1,
        string description = "Monthly rent for month of august",
        decimal amount = 24.99m,
        DateTime? createdAt = null)
    {
        return new Expense
        {
            Id = id,
            UserId = userId,
            Description = description,
            Amount = amount,
            CategoryId = 0,
            IsDeleted = false,
            CreatedAt = createdAt ?? DateTime.Now
        };
    }

    private static Income CreateIncome(
        int id = 1,
        int userId = 1,
        string description = "Monthly salary for month of august",
        decimal amount = 24.99m,
        DateTime? createdAt = null)
    {
        return new Income
        {
            Id = id,
            UserId = userId,
            Description = description,
            Amount = amount,
            CategoryId = 1,
            IsDeleted = false,
            CreatedAt = createdAt ?? DateTime.Now
        };
    }
}