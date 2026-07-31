using ExpenseTracker.Data;
using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests.Services;

public class ExpensesServiceTests
{
    [Fact]
    public async Task GetExpensesAsync_ReturnsExpenses_WhenExpensesExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Expenses = new List<Expense>
            {
                new()
                {
                    Description = "Breakfast",
                    Amount = 80.99m,
                    Category = "Food",
                    IsDeleted = false
                },
                new()
                {
                    Description = "Lunch",
                    Amount = 120.50m,
                    Category = "Food",
                    IsDeleted = false
                }
            }
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context);

        // Act
        var result = await service.GetExpensesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Breakfast", result[0].Description);
        Assert.Equal(120.50m, result[1].Amount);
    }

    [Fact]
    public async Task GetExpensesAsync_ReturnsEmptyList_WhenNoExpensesExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var service = new ExpenseService(context);

        // Act
        var result = await service.GetExpensesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExpenseByIdAsync_ReturnsExpense_WhenExpenseExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Expenses =
            [
                new Expense
            {
                Description = "Breakfast",
                Amount = 80.99m,
                Category = "Food",
                IsDeleted = false
            }
            ]
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new ExpenseService(context);

        var expenseId = user.Expenses.First().Id;

        // Act
        var result = await service.GetExpenseByIdAsync(expenseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(80.99m, result.Amount);
    }

    [Fact]
    public async Task GetExpenseByIdAsync_ReturnsNull_WhenExpenseDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var service = new ExpenseService(context);

        // Act
        var result = await service.GetExpenseByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateExpenseAsync_ReturnsExpense_WhenExpenseIsCreated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var service = new ExpenseService(context);

        var expense = new ExpenseReqDto
        {
            Description = "Dinner",
            Amount = 150.75m,
            Category = "Food"
        };

        var user = new User
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            FullName = "Test User",
            Username = "test",
            ContactNumber = "09876543210",
            HashedPassword = "password",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CreateExpenseAsync(user.Id, expense);

        // Assert - returned DTO
        Assert.NotNull(result);
        Assert.Equal("Dinner", result.Description);
        Assert.Equal(150.75m, result.Amount);
        Assert.Equal("Food", result.Category);

        // Assert - data persisted in database
        var savedExpense = await context.Expenses
            .FirstOrDefaultAsync(x => x.Id == result.Id);

        Assert.NotNull(savedExpense);
        Assert.Equal("Dinner", savedExpense.Description);
        Assert.Equal(150.75m, savedExpense.Amount);
        Assert.Equal("Food", savedExpense.Category);
    }

    [Fact]
    public async Task UpdateExpenseAsync_ReturnsUpdatedExpense_WhenExpenseExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var service = new ExpenseService(context);

        var user = new User
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            Expenses =
            [
                new Expense
                {
                    Description = "Dinner",
                    Amount = 150.75m,
                    Category = "Food"
                }
            ]
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var updatedExpense = new ExpenseReqDto
        {
            Description = "Breakfast",
            Amount = 49.99m,
        };

        var expenseId = user.Expenses.First().Id;
        var result = await service.UpdateExpenseAsync(expenseId, updatedExpense);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Breakfast", result.Description);
        Assert.Equal(49.99m, result.Amount);

        // Assert - data persisted in database
        var savedExpense = await context.Expenses
            .FirstOrDefaultAsync(x => x.Id == result.Id);

        Assert.NotNull(savedExpense);
        Assert.Equal("Breakfast", savedExpense.Description);
        Assert.Equal(49.99m, savedExpense.Amount);
        Assert.Equal("Food", savedExpense.Category);
    }

    [Fact]
    public async Task UpdateExpenseAsync_ReturnsNull_WhenExpenseDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var service = new ExpenseService(context);

        // Act
        var updatedExpense = new ExpenseReqDto
        {
            Description = "Breakfast",
            Amount = 49.99m,
        };

        var result = await service.UpdateExpenseAsync(Guid.NewGuid(), updatedExpense);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteExpenseAsync_ReturnsTrue_WhenExpenseIsDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var service = new ExpenseService(context);

        var user = new User
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            Expenses =
            [
                new Expense
                {
                    Description = "Dinner",
                    Amount = 150.75m,
                    Category = "Food"
                }
            ]
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var expenseId = user.Expenses.First().Id;
        var result = await service.DeleteExpenseAsync(expenseId);

        // Assert
        Assert.True(result);

        // Assert - data removed from database
        var deletedExpense = await context.Expenses
            .FirstOrDefaultAsync(x => x.Id == expenseId);
        Assert.Null(deletedExpense);
    }

    [Fact]
    public async Task DeleteExpenseAsync_ReturnFalse_WhenExpenseDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var service = new ExpenseService(context);

        // Act
        var result = await service.DeleteExpenseAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
