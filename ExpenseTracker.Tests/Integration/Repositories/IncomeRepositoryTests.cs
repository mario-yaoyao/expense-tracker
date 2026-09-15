
using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class IncomeRepositoryTests
    {
        [Fact]
        public async Task GetIncomesAsync_ReturnsAllNonDeletedIncomes()
        {
            // Arrange
            var secondIncomeId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();

            var incomes = new List<Income>
            {
                CreateIncome(isDeleted: true),
                CreateIncome(id: secondIncomeId),
            };

            context.Users.Add(user);
            context.Categories.Add(category);
            context.Incomes.AddRange(incomes);
            await context.SaveChangesAsync();

            //Act
            var result = await repository.GetAllIncomesAsync();

            // Assert
            Assert.Equal(2, result.totalCount);

            Assert.Contains(
                result.data,
                e => e.Description == incomes[1].Description
            );
        }

        [Fact]
        public async Task GetIncomesByUserAsync_ReturnsOnlyIncomesForSpecifiedUser()
        {
            // Arrange
            var secondIncomeId = 2;
            var thirdIncomeId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var users = new List<User>
            {
                CreateUser(username: "user1"),
                CreateUser(2, "user2")
            };

            var category = CreateCategory();

            var incomes = new List<Income>
            {
                CreateIncome(id: firstUserId, category.Id, isDeleted: true),
                CreateIncome(secondIncomeId, firstUserId, category.Id),
                CreateIncome(thirdIncomeId, secondUserId, category.Id),
            };

            context.Users.AddRange(users);
            context.Categories.Add(category);
            context.Incomes.AddRange(incomes);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetIncomesByUserAsync(firstUserId);

            // Assert
            Assert.Single(result.data);

            var income = result.data.Single();

            Assert.Equal(2, income.Id);
            Assert.Equal(firstUserId, income.UserId);
            Assert.False(income.IsDeleted);
            Assert.Equal(incomes[1].Description, income.Description);
        }

        [Fact]
        public async Task GetIncomeByUserAsync_ReturnsIncome_WhenIncomeExistsForUser()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();
            var income = CreateIncome();

            context.Users.Add(user);
            context.Categories.Add(category);

            context.Incomes.Add(income);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetIncomeByUserAsync(user.Id, income.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(income.Id, result.Id);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(income.Amount, result.Amount);
        }

        [Fact]
        public async Task GetIncomeByUserAsync_ReturnsNull_WhenIncomeDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetIncomeByUserAsync(userId, incomeId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetIncomeByIdAsync_ReturnsIncome_WhenIncomeExists()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();
            var income = CreateIncome();

            context.Users.Add(user);
            context.Categories.Add(category);
            context.Incomes.Add(income);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetIncomeByIdAsync(income.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(income.Id, result.Id);
            Assert.Equal(income.UserId, result.UserId);
        }

        [Fact]
        public async Task GetIncomeByIdAsync_ReturnsNull_WhenIncomeDoesNotExist()
        {
            // Arrange
            var incomeId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetIncomeByIdAsync(incomeId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddIncomeAsync_SavesIncomeToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();
            var income = CreateIncome();

            // Act
            await repository.AddIncomeAsync(income);

            // Assert
            var savedIncome = await context.Incomes.FindAsync(income.Id);

            Assert.Equal(income.UserId, savedIncome!.UserId);
            Assert.Equal(income.Amount, savedIncome.Amount);
            Assert.Equal(income.Description, savedIncome.Description);
        }

        // Helper Functions
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static IncomeRepository CreateRepository(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<IncomeRepository>>();

            return new IncomeRepository(context, mockLogger.Object);
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
            string name = "Salary")
        {
            return new Category
            {
                Id = id,
                Name = name
            };
        }

        private static Income CreateIncome(
            int id = 1,
            int userId = 1,
            int categoryId = 1,
            bool isDeleted = false)
        {
            return new Income
            {
                Id = id,
                UserId = userId,
                CategoryId = categoryId,
                Amount = 50m,
                Description = $"Income {id}",
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
