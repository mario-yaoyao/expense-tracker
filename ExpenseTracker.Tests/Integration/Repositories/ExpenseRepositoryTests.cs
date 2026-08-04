using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class ExpenseRepositoryTests
    {
        [Fact]
        public async Task GetAllExpensesAsync_ReturnsAllNonDeletedExpenses()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new ExpenseRepository(context);

            var existingExpenses = new List<Expense>
            {
                new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = 50.0m,
                    Description = "Test Expense 1",
                    IsDeleted = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = 67.30m,
                    Description = "Test Expense 2",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Expenses.AddRange(existingExpenses);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllExpensesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(existingExpenses[1].Description, result[0].Description);
        }

        [Fact]
        public async Task GetExpensesByUserAsync_ReturnsOnlyExpensesForSpecifiedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new ExpenseRepository(context);

            var existingExpenses = new List<Expense>
            {
                new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = 50.0m,
                    Description = "Test Expense 1",
                    IsDeleted = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = 50.0m,
                    Description = "Test Expense 2",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Amount = 100.0m,
                    Description = "Test Expense 3",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
            };

            context.Expenses.AddRange(existingExpenses);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetExpensesByUserAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal(existingExpenses[1].UserId, result[0].UserId);
        }

        [Fact]
        public async Task GetExpenseByUserAsync_ReturnsExpense_WhenExpenseExistsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new ExpenseRepository(context);

            var existingExpense = new Expense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 50.0m,
                Description = "Test Expense",
                CreatedAt = DateTime.UtcNow
            };

            context.Expenses.Add(existingExpense);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetExpenseByUserAsync(userId, existingExpense.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingExpense.Id, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(existingExpense.Amount, result.Amount);
        }

        [Fact]
        public async Task GetExpenseByUserAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new ExpenseRepository(context);

            // Act
            var result = await repository.GetExpenseByUserAsync(userId, expenseId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsExpense_WhenExpenseExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new ExpenseRepository(context);

            var existingExpense = new Expense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 50.0m,
                Description = "Test Expense",
                CreatedAt = DateTime.UtcNow
            };

            context.Expenses.Add(existingExpense);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetExpenseByIdAsync(existingExpense.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingExpense.Id, result.Id);
            Assert.Equal(existingExpense.UserId, result.UserId);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new ExpenseRepository(context);

            // Act
            var result = await repository.GetExpenseByIdAsync(expenseId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddExpenseAsync_SavesExpenseToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new ExpenseRepository(context);

            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = 100.0m,
                Description = "Test Expense",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repository.AddExpenseAsync(expense);

            // Assert
            var savedExpense = await context.Expenses.FindAsync(expense.Id);

            Assert.NotNull(savedExpense);
            Assert.Equal(userId, savedExpense.UserId);
            Assert.Equal(100.0m, savedExpense.Amount);
            Assert.Equal("Test Expense", savedExpense.Description);
        }
    }
}
