
using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class ProfileRepositoryTests
    {
        [Fact]
        public async Task GetUserProfileAsync_ReturnsOwnUserDetails()
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
        public async Task UpdatePasswordAsync_UpdatesPasswordAndUpdatedAt()
        {
            using var context = CreateContext();
            var repository = CreateRepository(context);

            var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);

            var user = CreateUser();

            user.HashedPassword = "oldpassword123";
            user.UpdatedAt = originalUpdatedAt;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.HashedPassword = "newpassword123";
            user.UpdatedAt = DateTime.UtcNow;

            // Act
            await repository.UpdatePasswordAsync(user);

            // Assert
            var updatedUser = await context.Users.FindAsync(1);

            Assert.NotNull(updatedUser);
            Assert.Equal("newpassword123", updatedUser!.HashedPassword);
            Assert.True(updatedUser.UpdatedAt > originalUpdatedAt);
        }

        // Helper Functions
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static ProfileRepository CreateRepository(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<ProfileRepository>>();

            return new ProfileRepository(context, mockLogger.Object);
        }

        private static User CreateUser(
            int id = 1,
            string username = "testuser")
        {
            return new User
            {
                Id = id,
                FullName = "Test User",
                Username = username,
                ContactNumber = "09123456789",
                HashedPassword = "password",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
