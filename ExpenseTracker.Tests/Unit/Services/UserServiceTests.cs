using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IMapper> mockMapper;

        public UserServiceTests()
        {
            mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetUserProfileAsync_ReturnsUserExpenses_WhenUserExist()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var expectedResponse = new UserResDto
            {
                Id = userId,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockRepo
                .Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync(user);

            mockMapper
                .Setup(x => x.Map<UserResDto>(user))
                .Returns(expectedResponse);

            var service = new UserService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.GetUserProfileAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.Username, result.Username);
        }

        [Fact]
        public async Task GetUserProfileAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var userId = Guid.NewGuid();

            var expectedResponse = new UserResDto
            {
                Id = userId,
                FullName = "Test User",
                Username = "testuser",
                ContactNumber = "09876543210",
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockRepo
                .Setup(x => x.GetUserProfileAsync(userId))
                .ReturnsAsync((User?)null);

            var service = new UserService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.GetUserProfileAsync(userId);

            // Assert
            Assert.Null(result);

            mockMapper.Verify(
                x => x.Map<UserResDto>(It.IsAny<User>()),
                Times.Never);
        }
    }
}
