using BudgetWise.DAL.Data;
using BudgetWise.DAL.Repositories;
using BudgetWise.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetWise.Tests.Integration.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ReturnsOwnUserDetails()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser();

            context.Users.Add(user);
            await context.SaveChangesAsync();

            //Act
            var result = await repository.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.FullName, result.FullName);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUserId = 999;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetUserByIdAsync(nonExistentUserId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsAllRecords()
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

            //Act
            var result = await repository.GetUsersAsync();

            // Assert
            Assert.Equal(4, result.totalCount);

            Assert.Contains(
                result.data,
                e => e.Username == users[1].Username
            );
        }

        // Helper Functions
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static UserRepository CreateRepository(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<UserRepository>>();

            return new UserRepository(context, mockLogger.Object);
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
    }
}
