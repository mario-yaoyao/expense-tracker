using AutoMapper;
using BudgetWise.BLL.Services;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Moq;

namespace BudgetWise.Tests.Unit.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task GetUsersAsync_ReturnsAllExpenses()
        {
            // Arrange
            var secondUserId = 2;
            var thirdUserId = 3;

            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var userService = CreateUserService(mockUserRepo, mockMapper);

            var paginationReq = CreatePaginationRequest();
            var users = new List<User>
            {
                CreateUser(),
                CreateUser(id: secondUserId, username: "testusertwo", isActive: false),
                CreateUser(id: thirdUserId, username: "testuserthree", createdAt: new DateTime(2026, 7, 15))
            };

            var mappedUsers = new List<UserResDto>
            {
                CreateMappedUser(),
                CreateMappedUser(users[1].Id, users[1].Username, users[1].Role, users[1].IsActive, users[1].CreatedAt),
                CreateMappedUser(users[2].Id, users[2].Username, users[2].Role, users[2].IsActive, users[2].CreatedAt)
            };

            var expectedResponse = (
                Data: users,
                TotalCount: 3,
                HasNextPage: false
            );

            mockUserRepo
                .Setup(x => x.GetUsersAsync(1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<UserResDto>>(It.IsAny<List<User>>()))
                .Returns(mappedUsers);

            // Act
            var (data, totalCount, _) = await userService.GetUsersAsync(paginationReq);

            // Assert
            Assert.Equal(expectedResponse.TotalCount, totalCount);
            Assert.Equal(mappedUsers[0].Username, data[0].Username);
            Assert.Equal(mappedUsers[0].Email, data[0].Email);
            Assert.Equal(mappedUsers[0].ContactNumber, data[0].ContactNumber);

            mockUserRepo.Verify(
                x => x.GetUsersAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenFound()
        {
            // Arrange
            var userId = 1;

            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var userService = CreateUserService(mockUserRepo, mockMapper);

            var user = CreateUser();
            var mappedUser = CreateMappedUser();

            mockMapper
                .Setup(x => x.Map<UserResDto>(It.IsAny<User>()))
                .Returns(mappedUser);

            mockUserRepo.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.FullName, result.FullName);

            mockUserRepo.Verify(
                x => x.GetUserByIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;

            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var userService = CreateUserService(mockUserRepo, mockMapper);

            mockUserRepo.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);

            mockUserRepo.Verify(
                x => x.GetUserByIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ReturnsTrue_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var username = "superadmin";

            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var userService = CreateUserService(mockUserRepo, mockMapper);

            var user = CreateUser();
            var mappedUser = CreateMappedUser();

            mockMapper
            .Setup(x => x.Map<UserResDto>(It.IsAny<User>()))
            .Returns(mappedUser);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await userService.ToggleUserStatusAsync(username, userId);

            // Assert
            Assert.True(result);

            mockUserRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var username = "superadmin";

            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var userService = CreateUserService(mockUserRepo, mockMapper);

            mockUserRepo.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await userService.ToggleUserStatusAsync(username, userId);

            // Assert
            Assert.False(result);
        }

        // Helper Functions
        private static UserService CreateUserService(
            Mock<IUserRepository>? mockUserRepo = null,
            Mock<IMapper>? mockMapper = null)
        {
            return new UserService(
                mockUserRepo!.Object,
                mockMapper!.Object);
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

        private static UserResDto CreateMappedUser(
            int id = 1,
            string username = "testuser",
            UserRole role = UserRole.User,
            bool isActive = true,
            DateTime? createdAt = null,
            DateTime? updatedAt = null)
        {
            return new UserResDto
            {
                Id = id,
                Username = username,
                FullName = "Test User",
                Email = $"{username}@gmail.com",
                ContactNumber = "09123456789",
                Role = role,
                IsActive = isActive,
                CreatedAt = createdAt ?? DateTime.UtcNow,
                UpdatedAt = updatedAt ?? DateTime.UtcNow
            };
        }

        private static Category CreateCategory(
            int id = 1,
            string name = "Transportation")
        {
            return new Category
            {
                Id = id,
                Name = name
            };
        }

        private static Expense CreateExpense(
            int id,
            int userId,
            Category category,
            bool isDeleted = false)
        {
            return new Expense
            {
                Id = id,
                UserId = userId,
                Description = $"Expense {id}",
                Amount = 50m,
                CategoryId = category.Id,
                Category = category,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }

        private static UserQueryReqDto CreatePaginationRequest(
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new UserQueryReqDto
            {
                Page = page,
                Limit = limit,
                Search = search
            };
        }
    }
}
