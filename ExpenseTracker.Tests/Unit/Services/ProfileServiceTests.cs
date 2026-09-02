using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class ProfileServiceTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserExpenses_WhenUserExist()
        {
            // Arrange
            var userId = 1;

            var mockRepo = new Mock<IProfileRepository>();
            var service = new ProfileService(mockRepo.Object, mockMapper.Object);

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
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            mockMapper
                .Setup(x => x.Map<UserResDto>(user))
                .Returns(expectedResponse);

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
            var userId = 1;

            var mockRepo = new Mock<IProfileRepository>();
            var service = new ProfileService(mockRepo.Object, mockMapper.Object);

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
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await service.GetUserProfileAsync(userId);

            // Assert
            Assert.Null(result);

            mockMapper.Verify(
                x => x.Map<UserResDto>(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsSuccess_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "oldpassword123";
            var newPassword = "newpassword123";

            var mockRepo = new Mock<IProfileRepository>();
            var service = new ProfileService(mockRepo.Object, mockMapper.Object);

            var user = new User
            {
                Id = userId,
                Username = "testuser",
            };

            user.HashedPassword = new PasswordHasher<User>()
                .HashPassword(user, currentPassword);

            var request = new ChangePasswordReqDto
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword
            };

            mockRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            mockRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.ChangePasswordAsync(userId, request);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);

            mockRepo.Verify(
                x => x.UpdatePasswordAsync(It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFailure_WhenUserNotFound()
        {
            // Arrange
            var userId = 1;

            var mockRepo = new Mock<IProfileRepository>();
            var service = new ProfileService(mockRepo.Object, mockMapper.Object);

            var request = ChangePassword();

            mockRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            mockRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.ChangePasswordAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("User not found.", result.ErrorMessage);

            mockRepo.Verify(
                x => x.UpdatePasswordAsync(It.IsAny<User>()),
                Times.Never);
        }

        // new
        [Fact]
        public async Task ChangePasswordAsync_ReturnsFailure_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "oldpassword123";

            var mockRepo = new Mock<IProfileRepository>();

            var request = ChangePassword(currentPassword: "wrongpassword123");

            var user = new User
            {
                Id = userId,
                Username = "testuser",
            };

            user.HashedPassword = new PasswordHasher<User>()
                .HashPassword(user, currentPassword);

            mockRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            mockRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var service = new ProfileService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.ChangePasswordAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("Current password is incorrect.", result.ErrorMessage);

            mockRepo.Verify(
                x => x.UpdatePasswordAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFailure_WhenNewPasswordMatchesCurrentPassword()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "oldpassword123";

            var mockRepo = new Mock<IProfileRepository>();
            var service = new ProfileService(mockRepo.Object, mockMapper.Object);

            var request = ChangePassword(newPassword: "oldpassword123");

            var user = new User
            {
                Id = userId,
                Username = "testuser",
            };

            user.HashedPassword = new PasswordHasher<User>()
                .HashPassword(user, currentPassword);

            mockRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            mockRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.ChangePasswordAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.Data);
            Assert.Equal("New password must be different from your current password.", result.ErrorMessage);

            mockRepo.Verify(
                x => x.UpdatePasswordAsync(It.IsAny<User>()),
                Times.Never); ;
        }

        // Helper Functions
        private readonly Mock<IMapper> mockMapper;

        public ProfileServiceTests()
        {
            mockMapper = new Mock<IMapper>();
        }


        private static ChangePasswordReqDto ChangePassword(
            string currentPassword = "oldpassword123",
            string newPassword = "newpassword123")
        {
            return new ChangePasswordReqDto
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = newPassword
            };
        }
    }
}
