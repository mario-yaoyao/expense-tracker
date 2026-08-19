using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class ExpenseRepositoryTests
    {
        [Fact]
        public async Task GetAllExpensesAsync_ReturnsAllNonDeletedExpenses()
        {
            // Arrange
            var firstExpenseId = 1;
            var secondExpenseId = 2;
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();

            context.Users.Add(user);
            context.Categories.Add(category);

            var expenses = new List<Expense>
            {
                CreateExpense(firstExpenseId, userId, category.Id, true),
                CreateExpense(secondExpenseId, userId, category.Id),
            };

            context.Expenses.AddRange(expenses);
            await context.SaveChangesAsync();

            //Act
            var result = await repository.GetAllExpensesAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expenses[1].Description, result[0].Description);
        }

        [Fact]
        public async Task GetExpensesByUserAsync_ReturnsOnlyExpensesForSpecifiedUser()
        {
            // Arrange
            var firstExpenseId = 1;
            var secondExpenseId = 2;
            var thirdExpenseId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var firstUser = CreateUser(1, "user1");
            var secondUser = CreateUser(2, "user2");
            var category = CreateCategory();

            context.Users.Add(firstUser);
            context.Users.Add(secondUser);
            context.Categories.Add(category);

            var expenses = new List<Expense>
            {
                CreateExpense(firstExpenseId, firstUserId, category.Id, true),
                CreateExpense(secondExpenseId, firstUserId, category.Id),
                CreateExpense(thirdExpenseId, secondUserId, category.Id),
            };

            context.Expenses.AddRange(expenses);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetExpensesByUserAsync(firstUserId);

            // Assert
            Assert.Single(result);

            var expense = result.Single();

            Assert.Equal(2, expense.Id);
            Assert.Equal(firstUserId, expense.UserId);
            Assert.False(expense.IsDeleted);
            Assert.Equal("Expense 2", expense.Description);
        }

        [Fact]
        public async Task GetExpenseByUserAsync_ReturnsExpense_WhenExpenseExistsForUser()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();

            context.Users.Add(user);
            context.Categories.Add(category);

            var expense = CreateExpense(userId, user.Id, category.Id);

            context.Expenses.Add(expense);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetExpenseByUserAsync(userId, expense.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expense.Id, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(expense.Amount, result.Amount);
        }

        [Fact]
        public async Task GetExpenseByUserAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetExpenseByUserAsync(userId, expenseId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsExpense_WhenExpenseExists()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();

            context.Users.Add(user);
            context.Categories.Add(category);

            var expense = CreateExpense(1, user.Id, category.Id);

            context.Expenses.Add(expense);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetExpenseByIdAsync(expense.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expense.Id, result.Id);
            Assert.Equal(expense.UserId, result.UserId);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var expenseId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetExpenseByIdAsync(expenseId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddExpenseAsync_SavesExpenseToDatabase()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();

            var expense = CreateExpense(userId, user.Id, category.Id);

            // Act
            await repository.AddExpenseAsync(expense);

            // Assert
            var savedExpense = await context.Expenses.FindAsync(expense.Id);

            Assert.Equal(expense.UserId, savedExpense!.UserId);
            Assert.Equal(expense.Amount, savedExpense.Amount);
            Assert.Equal(expense.Description, savedExpense.Description);
        }
    
        // Helper Functions
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static ExpenseRepository CreateRepository(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<ExpenseRepository>>();

            return new ExpenseRepository(context, mockLogger.Object);
        }

        private static User CreateUser(
            int id = 1,
            string username = "testuser")
        {
            return new User
            {
                Id = id,
                Username = username,
                FullName = "Test User",
                ContactNumber = "09123456789",
                HashedPassword = "password",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static Category CreateCategory(
            int id = 1,
            string name = "Transportation")
        {
            return new Category
            {
                Id = id,
                Name = name
            };
        }

        private static Expense CreateExpense(
            int id,
            int userId,
            int categoryId,
            bool isDeleted = false)
        {
            return new Expense
            {
                Id = id,
                UserId = userId,
                CategoryId = categoryId,
                Amount = 50m,
                Description = $"Expense {id}",
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
