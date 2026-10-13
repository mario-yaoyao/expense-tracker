using BudgetWise.DAL.Data;
using BudgetWise.DAL.Repositories;
using BudgetWise.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetWise.Tests.Integration.Repositories
{
    public class TransactionRepositoryTests
    {
        [Fact]
        public async Task GetAllTransactionsAsync_ReturnsAllTransactions()
        {
            // Arrange
            var secondTransactionId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);
            var user = CreateUser();

            var transactions = new List<Transaction>
            {
                CreateTransaction(),
                CreateTransaction(id: secondTransactionId),
            };

            context.Users.Add(user);
            context.Transactions.AddRange(transactions);
            await context.SaveChangesAsync();

            //Act
            var (data, _) = await repository.GetAllTransactionsAsync();

            // Assert
            Assert.Equal(2, data.Count);

            Assert.Contains(
                data,
                e => e.Username == transactions[1].Username
            );
        }

        [Fact]
        public async Task GetTransactionsByUserAsync_ReturnsOnlyTransactionsForSpecifiedUser()
        {
            // Arrange
            var secondTransactionId = 2;
            var thirdTransactionId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var users = new List<User>
            {
                CreateUser(username: "user1"),
                CreateUser(secondUserId, "user2")
            };

            var transactions = new List<Transaction>
            {
                CreateTransaction(),
                CreateTransaction(id: secondTransactionId, message: "Updated category", type: TransactionType.Update, activity: "Updated category"),
                CreateTransaction(id: thirdTransactionId, userId: users[1].Id, message: "Deleted category", type: TransactionType.Update, activity: "Deleted category"),
            };

            context.Users.AddRange(users);
            context.Transactions.AddRange(transactions);
            await context.SaveChangesAsync();

            // Act
            var (data, _) = await repository.GetTransactionsByUserAsync(firstUserId);

            // Assert
            Assert.NotNull(data);
            Assert.Equal(transactions[1].Type, data[0].Type);
            Assert.Equal(transactions[1].Message, data[0].Message);
        }

        [Fact]
        public async Task GetTransactionAsync_ReturnsTransaction_WhenTransactionExistsForUser()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var transaction = CreateTransaction();

            context.Users.Add(user);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetTransactionAsync(user.Id, transaction.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transaction.Id, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(transaction.Type, result.Type);
        }

        [Fact]
        public async Task GetTransactionAsync_ReturnsNull_WhenTransactionDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var transactionId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetTransactionAsync(userId, transactionId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOwnTransactionAsync_ReturnsTransaction_WhenTransactionExists()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var transaction = CreateTransaction();

            context.Users.Add(user);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetOwnTransactionAsync(transaction.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transaction.Id, result.Id);
            Assert.Equal(transaction.UserId, result.UserId);
        }

        [Fact]
        public async Task GetOwnTransactionAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var transactionId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetOwnTransactionAsync(transactionId);

            // Assert
            Assert.Null(result);
        }

        // Helper Functions
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static TransactionRepository CreateRepository(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<TransactionRepository>>();

            return new TransactionRepository(context, mockLogger.Object);
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

        private static Transaction CreateTransaction(
            int id = 1,
            string message = "Created category",
            string level = "Information",
            DateTime? timestamp = null,
            string? exception = null,
            int? userId = 1,
            string? username = "testuser",
            TransactionType? type = TransactionType.Create,
            string? entityName = "Category",
            string? activity = "Created category")
        {
            return new Transaction
            {
                Id = id,
                Message = message,
                Level = level,
                TimeStamp = timestamp ?? DateTime.UtcNow,
                Exception = exception,
                UserId = userId,
                Username = username,
                Type = type,
                EntityName = entityName,
                Activity = activity
            };
        }
    }
}
