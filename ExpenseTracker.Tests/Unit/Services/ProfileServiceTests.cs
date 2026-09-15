using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class ProfileServiceTests
    {
        [Fact]
        public async Task ChangePasswordAsync_ReturnsSuccess_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "oldpassword123";
            var newPassword = "newpassword123";

            var mockProfileRepo = new Mock<IProfileRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var profileService = CreateProfileService(mockProfileRepo, mockUserRepo, mockMapper);

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

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            mockProfileRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await profileService.ChangePasswordAsync(userId, request);

            // Assert
            Assert.True(result.Success);

            mockProfileRepo.Verify(
                x => x.UpdatePasswordAsync(It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFailure_WhenUserNotFound()
        {
            // Arrange
            var userId = 1;

            var mockProfileRepo = new Mock<IProfileRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var profileService = CreateProfileService(mockProfileRepo, mockUserRepo, mockMapper);

            var request = ChangePassword();

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            mockProfileRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await profileService.ChangePasswordAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.ErrorMessage);

            mockProfileRepo.Verify(
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

            var mockProfileRepo = new Mock<IProfileRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var profileService = CreateProfileService(mockProfileRepo, mockUserRepo, mockMapper);

            var request = ChangePassword(currentPassword: "wrongpassword123");

            var user = new User
            {
                Id = userId,
                Username = "testuser",
            };

            user.HashedPassword = new PasswordHasher<User>()
                .HashPassword(user, currentPassword);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            mockProfileRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await profileService.ChangePasswordAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Current password is incorrect.", result.ErrorMessage);

            mockProfileRepo.Verify(
                x => x.UpdatePasswordAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFailure_WhenNewPasswordMatchesCurrentPassword()
        {
            // Arrange
            var userId = 1;
            var currentPassword = "oldpassword123";

            var mockProfileRepo = new Mock<IProfileRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var profileService = CreateProfileService(mockProfileRepo, mockUserRepo, mockMapper);

            var request = ChangePassword(newPassword: "oldpassword123");

            var user = new User
            {
                Id = userId,
                Username = "testuser",
            };

            user.HashedPassword = new PasswordHasher<User>()
                .HashPassword(user, currentPassword);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            mockProfileRepo
                .Setup(x => x.UpdatePasswordAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await profileService.ChangePasswordAsync(userId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("New password must be different from your current password.", result.ErrorMessage);

            mockProfileRepo.Verify(
                x => x.UpdatePasswordAsync(It.IsAny<User>()),
                Times.Never); ;
        }

        // Helper Functions
        private ProfileService CreateProfileService(
            Mock<IProfileRepository>? mockProfileRepo = null,
            Mock<IUserRepository>? mockUserRepo = null,
            Mock<IMapper>? mockMapper = null)
        {
            return new ProfileService(
                mockProfileRepo!.Object,
                mockUserRepo!.Object,
                mockMapper!.Object);
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
