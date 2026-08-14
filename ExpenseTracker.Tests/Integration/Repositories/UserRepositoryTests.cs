
using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetUserProfileAsync_ReturnsOwnUserDetails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var mockLogger = new Mock<ILogger<UserRepository>>();

            var repository = new UserRepository(context, mockLogger.Object);

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09123456789",
                HashedPassword = "password",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            //Act
            var result = await repository.GetUserProfileAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.FullName, result.FullName);
        }
    }
}
