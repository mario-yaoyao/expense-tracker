using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class CategoryServiceTests
    {

        private readonly Mock<IMapper> mockMapper;

        public CategoryServiceTests()
        {
            mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsUserCategories_WhenRoleIsUser()
        {
            // Arrange
            var firstCategoryId = 1;
            var secondCategoryId = 2;
            var userId = 1;
            var categoryType = CategoryType.Expense;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            var request = new List<Category>
            {
                CreateCategory(firstCategoryId, userId, "Grocery", categoryType),
                CreateCategory(secondCategoryId, userId, "Transportation", categoryType),
            };
            var queryRequest = CreateQueryRequest();

            var expectedResponseData = new List<CategoryResDto>
            {
                new()
                {
                    Id = request[0].Id,
                    UserId = request[0].UserId,
                    Name = request[0].Name,
                    Type = request[0].Type,
                    IsDeleted = request[0].IsDeleted,
                    CreatedAt = request[0].CreatedAt,
                    UpdatedAt = request[0].UpdatedAt,
                },
                new()
                {
                    Id = request[1].Id,
                    UserId = request[1].UserId,
                    Name = request[1].Name,
                    Type = request[1].Type,
                    IsDeleted = request[1].IsDeleted,
                    CreatedAt = request[1].CreatedAt,
                    UpdatedAt = request[1].UpdatedAt,
                }
            };

            var expectedResponse = (
                Data: request,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetCategoriesByUserAsync(userId, queryRequest.Type, queryRequest.Page, queryRequest.Limit, queryRequest.Search))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<CategoryResDto>>(It.IsAny<List<Category>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetCategoriesAsync(userId, "User", queryRequest);

            // Assert
            Assert.Equal(expectedResponseData[0].Name, result.data[0].Name);
            Assert.Equal(CategoryType.Expense, result.data[1].Type);

            mockRepo.Verify(
                x => x.GetCategoriesByUserAsync(userId, categoryType, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsAllCategories_WhenRoleIsNotUser()
        {
            // Arrange
            var firstCategoryId = 1;
            var secondCategoryId = 2;
            var thirdCategoryId = 3;
            var firstUserId = 1;
            var secondUserId = 2;
            var categoryType = CategoryType.Expense;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            var request = new List<Category>
            {
                CreateCategory(firstCategoryId, firstUserId, "Grocery", categoryType),
                CreateCategory(secondCategoryId, firstUserId, "Transportation", categoryType),
                CreateCategory(thirdCategoryId, secondUserId, "Internet", categoryType),
            };
            var queryRequest = CreateQueryRequest();

            var expectedResponseData = new List<CategoryResDto>
            {
                new()
                {
                    Id = request[0].Id,
                    UserId = request[0].UserId,
                    Name = request[0].Name,
                    Type = request[0].Type,
                },
                new()
                {
                    Id = request[1].Id,
                    UserId = request[1].UserId,
                    Name = request[1].Name,
                    Type = request[1].Type,
                },
                new()
                {
                    Id = request[2].Id,
                    UserId = request[2].UserId,
                    Name = request[2].Name,
                    Type = request[2].Type,
                }
            };

            var expectedResponse = (
                Data: request,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetAllCategoriesAsync(queryRequest.Page, queryRequest.Limit, queryRequest.Search))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<CategoryResDto>>(It.IsAny<List<Category>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetCategoriesAsync(firstUserId, "SuperAdmin", queryRequest);

            // Assert
            Assert.Equal(expectedResponseData[0].Name, result.data[0].Name);
            Assert.Equal(expectedResponseData[1].Type, result.data[1].Type);

            mockRepo.Verify(
                x => x.GetAllCategoriesAsync(1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsCategory_WhenFound()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            var expectedResponse = new Category
            {
                Id = categoryId,
                UserId = userId,
                Name = "Grocery",
                Type = CategoryType.Expense,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var expectedResDto = new CategoryResDto
            {
                Id = expectedResponse.Id,
                UserId = expectedResponse.UserId,
                Name = expectedResponse.Name,
                Type = expectedResponse.Type,
                CreatedAt = expectedResponse.CreatedAt,
                UpdatedAt = expectedResponse.UpdatedAt
            };

            mockMapper
                .Setup(x => x.Map<CategoryResDto>(It.IsAny<Category>()))
                .Returns(expectedResDto);

            mockRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await service.GetCategoryByIdAsync(userId, "User", categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.Id);
            Assert.Equal(expectedResponse.Name, result.Name);
            Assert.Equal(expectedResponse.Type, result.Type);

            mockRepo.Verify(
                x => x.GetCategoryByUserAsync(userId, categoryId),
                Times.Once);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await service.GetCategoryByIdAsync(userId, "User", categoryId);

            // Assert
            Assert.Null(result);

            mockRepo.Verify(
                x => x.GetCategoryByUserAsync(userId, categoryId),
                Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_ReturnsCreatedCategory_WhenRequestIsValid()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            var request = CreateCategoryRequest();

            var expectedResponseDto = new CategoryResDto
            {
                Id = categoryId,
                UserId = userId,
                Name = request.Name,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockMapper
                .Setup(x => x.Map<CategoryResDto>(It.IsAny<Category>()))
                .Returns(expectedResponseDto);

            // Act
            var result = await service.CreateCategoryAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Type, result.Type);

            mockRepo.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsUpdatedCategory_WhenCategoryExists()
        {
            // Arrange
            var firstCategoryId = 1;
            var userId = 1;

            var mockRepo = new Mock<ICategoryRepository>();

            var request = UpdateCategory();

            var existingCategory = new Category
            {
                Id = firstCategoryId,
                UserId = userId,
                Name = request.Name,
                Type = request.Type!.Value,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var expectedResponseDto = new CategoryResDto
            {
                Id = firstCategoryId,
                UserId = userId,
                Name = existingCategory.Name,
                Type = existingCategory.Type,
                CreatedAt = existingCategory.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            mockMapper
                .Setup(x => x.Map<CategoryResDto>(It.IsAny<Category>()))
                .Returns(expectedResponseDto);

            mockRepo.Setup(x => x.GetCategoryByUserAsync(userId, existingCategory.Id))
                .ReturnsAsync(existingCategory);

            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.UpdateCategoryAsync(userId, existingCategory.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UpdatedAt);
            Assert.True(result.UpdatedAt > result.CreatedAt);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(expectedResponseDto.Name, result.Name);
            Assert.Equal(expectedResponseDto.Type, result.Type);

            mockRepo.Verify(
                x => x.GetCategoryByUserAsync(userId, existingCategory.Id),
                Times.Once);

            mockRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            var request = UpdateCategory();

            mockRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await service.UpdateCategoryAsync(userId, categoryId, request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsTrue_WhenExpenseExists()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            var existingCategory = new Category
            {
                Id = categoryId,
                UserId = userId,
                Name = "Grocery",
                Type = CategoryType.Expense,
                IsDeleted = false
            };

            mockRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync(existingCategory);

            // Act
            var result = await service.DeleteCategoryAsync(userId, categoryId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await service.DeleteCategoryAsync(userId, categoryId);

            // Assert
            Assert.False(result);
        }

        // Helper Functions
        private static Category CreateCategory(
            int id,
            int userId,
            string name,
            CategoryType type,
            bool isDeleted = false)
        {
            return new Category
            {
                Id = id,
                UserId = userId,
                Name = name,
                Type = type,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }

        private static UpdateCategoryReqDto UpdateCategory(
            string name = "Rent",
            CategoryType type = CategoryType.Expense)
        {
            return new UpdateCategoryReqDto
            {
                Name = name,
                Type = type
            };
        }

        private static CreateCategoryReqDto CreateCategoryRequest(
            string name = "Rent",
            CategoryType type = CategoryType.Expense)
        {
            return new CreateCategoryReqDto
            {
                Name = name,
                Type = type
            };
        }

        private static CategoryQueryReqDto CreateQueryRequest(
            CategoryType type = CategoryType.Expense,
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new CategoryQueryReqDto
            {
                Type = type,
                Page = page,
                Limit = limit,
                Search = search
            };
        }
    }
}
