using BudgetWise.DAL.Data;
using BudgetWise.DAL.Repositories;
using BudgetWise.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetWise.Tests.Integration.Repositories
{
    public class CategoryRepositoryTests
    {
        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            // Arrange
            var secondCategoryId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);
            var user = CreateUser();

            var categories = new List<Category>
            {
                CreateCategory(isDeleted: true),
                CreateCategory(id: secondCategoryId),
            };

            context.Users.Add(user);
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
            var secondCategoryId = 2;
            var thirdCategoryId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var users = new List<User>
            {
                CreateUser(username: "user1"),
                CreateUser(secondUserId, "user2")
            };

            var categories = new List<Category>
            {
                CreateCategory(isDeleted: true),
                CreateCategory(id: secondCategoryId, name: "Grocery"),
                CreateCategory(id: thirdCategoryId, userId: users[1].Id, name: "Rent"),
            };

            context.Users.AddRange(users);
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Act
            var (data, _) = await repository.GetCategoriesByUserAsync(firstUserId);

            // Assert
            Assert.Single(data);

            var expense = data.Single();

            Assert.Equal(2, expense.Id);
            Assert.Equal(firstUserId, expense.UserId);
            Assert.False(expense.IsDeleted);
            Assert.Equal("Grocery", expense.Name);
        }

        [Fact]
        public async Task GetCategoryByUserAsync_ReturnsCategory_WhenCategoryExistsForUser()
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
            var result = await repository.GetCategoryByUserAsync(user.Id, category.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(user.Id, result.UserId);
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
        public async Task GetCategoryByIdAsync_ReturnsNull_WhenExpenseDoesNotExist()
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
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();
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
