using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class CategoryRepositoryTests
    {
        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllNonDeletedCategories()
        {
            // Arrange
            var firstCategoryId = 1;
            var secondCategoryId = 2;
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();

            context.Users.Add(user);

            var categories = new List<Category>
            {
                CreateCategory(id: firstCategoryId, userId: userId, isDeleted: true),
                CreateCategory(id: secondCategoryId, userId: userId),
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            //Act
            var (data, hasNextPage) = await repository.GetAllCategoriesAsync();

            // Assert
            Assert.Equal(2, data.Count);

            Assert.Contains(
                data,
                e => e.Name == categories[1].Name
            );
        }

        [Fact]
        public async Task GetCategoriesByUserAsync_ReturnsOnlyCategoriesForSpecifiedUser()
        {
            // Arrange
            var firstCategoryId = 1;
            var secondCategoryId = 2;
            var thirdCategoryId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var firstUser = CreateUser(1, "user1");
            var secondUser = CreateUser(2, "user2");

            context.Users.Add(firstUser);
            context.Users.Add(secondUser);

            var categories = new List<Category>
            {
                CreateCategory(id: firstCategoryId, userId: firstUserId, isDeleted: true),
                CreateCategory(id: secondCategoryId, userId: firstUserId, name: "Grocery"),
                CreateCategory(id: thirdCategoryId, userId: secondUserId, name: "Rent"),
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetCategoriesByUserAsync(firstUserId);

            // Assert
            Assert.Single(result.data);

            var expense = result.data.Single();

            Assert.Equal(2, expense.Id);
            Assert.Equal(firstUserId, expense.UserId);
            Assert.False(expense.IsDeleted);
            Assert.Equal("Grocery", expense.Name);
        }

        [Fact]
        public async Task GetCategoryByUserAsync_ReturnsCategory_WhenExpenseExistsForUser()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();

            context.Users.Add(user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetCategoryByUserAsync(userId, category.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(category.Name, result.Name);
        }

        [Fact]
        public async Task GetCategoryByUserAsync_ReturnsNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var categoryId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetCategoryByUserAsync(userId, categoryId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsCategory_WhenCategoryExists()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            var category = CreateCategory();

            context.Users.Add(user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetCategoryByIdAsync(category.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.UserId, result.UserId);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var categoryId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddCategoryAsync_SavesCategoryToDatabase()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
            //var category = CreateCategory();

            var category = CreateCategory();

            // Act
            await repository.AddCategoryAsync(category);

            // Assert
            var savedExpense = await context.Categories.FindAsync(category.Id);

            Assert.Equal(category.UserId, savedExpense!.UserId);
            Assert.Equal(category.Name, savedExpense.Name);
            Assert.Equal(category.Type, savedExpense.Type);
        }

        // Helper Functions
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static CategoryRepository CreateRepository(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<CategoryRepository>>();

            return new CategoryRepository(context, mockLogger.Object);
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
            int userId = 1,
            string name = "Transportation",
            CategoryType type = CategoryType.Expense,
            bool isDeleted = false
            )
        {
            return new Category
            {
                Id = id,
                UserId = userId,
                Name = name,
                Type = type,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }
    }
}
