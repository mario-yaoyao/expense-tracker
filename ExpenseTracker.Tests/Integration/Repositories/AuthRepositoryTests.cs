using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class AuthRepositoryTests
    {
        [Fact]
        public async Task GetByUsernameAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser(userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByUsernameAsync(user.Username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task GetByUsernameAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = CreateContext();
            var repository = CreateRepository(context);

            // Act
            var result = await repository.GetByUsernameAsync("unkownuser");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser(userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task IsUsernameTakenAsync_ReturnsTrue_WhenUsernameExists()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser(userId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.IsUsernameTakenAsync(user.Username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddUserAsync_AddsUserSuccessfully()
        {
            // Arrange
            var userId = 1;

            using var context = CreateContext();
            var repository = CreateRepository(context);

            var user = CreateUser(userId);

            // Act
            await repository.AddUserAsync(user);

            // Assert
            var savedUser = await context.Users.FindAsync(userId);

            Assert.NotNull(savedUser);
            Assert.Equal(user.Username, savedUser.Username);
            Assert.Equal(user.FullName, savedUser.FullName);
        }

        // Helper Functions
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static AuthRepository CreateRepository(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<AuthRepository>>();

            return new AuthRepository(context, mockLogger.Object);
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
    }
}
