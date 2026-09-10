using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task GetUsersAsync_ReturnsAllExpenses()
        {
            // Arrange
            var secondUserId = 2;
            var thirdUserId = 3;

            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object, mockMapper.Object);

            var users = new List<User>
            {
                CreateUser(),
                CreateUser(id: secondUserId, username: "testusertwo", isActive: false),
                CreateUser(id: thirdUserId, username: "testuserthree", createdAt: new DateTime(2026, 7, 15))
            };

            var paginationReq = CreatePaginationRequest();

            var mappedUsers = new List<UserResDto>
            {
                new()
                {
                    Id = users[0].Id,
                    FullName = users[0].FullName,
                    Username = users[0].Username,
                    Email = users[0].Email,
                    ContactNumber = users[0].ContactNumber,
                    Role = users[0].Role,
                    IsActive = users[0].IsActive,
                    CreatedAt = users[0].CreatedAt,
                    UpdatedAt = users[0].UpdatedAt,
                },
                new()
                {
                    Id = users[1].Id,
                    FullName = users[1].FullName,
                    Username = users[1].Username,
                    Email = users[1].Email,
                    ContactNumber = users[1].ContactNumber,
                    Role = users[1].Role,
                    IsActive = users[1].IsActive,
                    CreatedAt = users[1].CreatedAt,
                    UpdatedAt = users[1].UpdatedAt,
                },
                new()
                {
                   Id = users[2].Id,
                    FullName = users[2].FullName,
                    Username = users[2].Username,
                    Email = users[2].Email,
                    ContactNumber = users[2].ContactNumber,
                    Role = users[2].Role,
                    IsActive = users[2].IsActive,
                    CreatedAt = users[2].CreatedAt,
                    UpdatedAt = users[2].UpdatedAt,
                }
            };

            var expectedResponse = (
                Data: users,
                TotalCount: 3,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetUsersAsync(1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<UserResDto>>(It.IsAny<List<User>>()))
                .Returns(mappedUsers);

            // Act
            var result = await service.GetUsersAsync(paginationReq);

            // Assert
            Assert.Equal(expectedResponse.TotalCount, result.totalCount);
            Assert.Equal(mappedUsers[0].Username, result.data[0].Username);
            Assert.Equal(mappedUsers[0].Email, result.data[0].Email);
            Assert.Equal(mappedUsers[0].ContactNumber, result.data[0].ContactNumber);

            mockRepo.Verify(
                x => x.GetUsersAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenFound()
        {
            // Arrange
            var userId = 1;

            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object, mockMapper.Object);

            var user = CreateUser();

            var mappedUser = new UserResDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                ContactNumber = user.ContactNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
            };

            mockMapper
                .Setup(x => x.Map<UserResDto>(It.IsAny<User>()))
                .Returns(mappedUser);

            mockRepo.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await service.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.FullName, result.FullName);

            mockRepo.Verify(
                x => x.GetUserByIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;

            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await service.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);

            mockRepo.Verify(
                x => x.GetUserByIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ReturnsUpdatedUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var username = "superadmin";

            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object, mockMapper.Object);

            var user = CreateUser();

            mockMapper
                .Setup(x => x.Map<UserResDto>(It.IsAny<User>()))
                .Returns((User u) => new UserResDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    ContactNumber = u.ContactNumber,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                });

            mockRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await service.ToggleUserStatusAsync(username, userId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UpdatedAt);
            Assert.True(result.UpdatedAt > result.CreatedAt);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.ContactNumber, result.ContactNumber);

            mockRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatusAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var username = "superadmin";

            var mockRepo = new Mock<IUserRepository>();
            var service = new UserService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await service.ToggleUserStatusAsync(username, userId);

            // Assert
            Assert.Null(result);
        }

        // Helper Functions
        private readonly Mock<IMapper> mockMapper;

        public UserServiceTests()
        {
            mockMapper = new Mock<IMapper>();
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

        //private static UpdateExpenseReqDto UpdateExpense(
        //    string description = "Updated Expense",
        //    decimal amount = 450m,
        //    int categoryId = 1)
        //{
        //    return new UpdateExpenseReqDto
        //    {
        //        Description = description,
        //        Amount = amount,
        //        CategoryId = categoryId,
        //    };
        //}

        //private static CreateExpenseReqDto CreateExpenseRequest(
        //    string description = "Expense 4",
        //    decimal amount = 400m,
        //    int categoryId = 1)
        //{
        //    return new CreateExpenseReqDto
        //    {
        //        Description = description,
        //        Amount = amount,
        //        CategoryId = categoryId
        //    };
        //}

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
