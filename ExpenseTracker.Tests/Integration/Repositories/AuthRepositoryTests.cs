using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests.Integration.Repositories
{
    public class AuthRepositoryTests
    {
        [Fact]
        public async Task GetByUsernameAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var user = new User
            {
                Username = "testuser",
                FullName = "Test User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository = new AuthRepository(context);

            // Act
            var result = await repository.GetByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task GetByUsernameAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new AuthRepository(context);

            // Act
            var result = await repository.GetByUsernameAsync("testuser");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new AuthRepository(context);

            var existingUser = new User
            {
                Id = userId,
                Username = "testuser",
                FullName = "Test User"
            };

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingUser.Id, result.Id);
            Assert.Equal(existingUser.Username, result.Username);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new AuthRepository(context);

            // Act
            var result = await repository.GetByUsernameAsync("testuser");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task IsUsernameTakenAsync_ReturnsTrue_WhenUsernameExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new AuthRepository(context);

            var existingUser = new User
            {
                Id = userId,
                Username = "testuser",
                FullName = "Test User"
            };

            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.IsUsernameTakenAsync("testuser");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddUserAsync_AddsUserSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var repository = new AuthRepository(context);

            var user = new User
            {
                Id = userId,
                Username = "testuser",
                FullName = "Test User"
            };

            // Act
            await repository.AddUserAsync(user);

            // Assert
            var savedUser = await context.Users.FindAsync(userId);

            Assert.NotNull(savedUser);
            Assert.Equal(user.Username, savedUser.Username);
            Assert.Equal(user.FullName, savedUser.FullName);
        }
    }
}
