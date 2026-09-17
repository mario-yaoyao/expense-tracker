using AutoMapper;
using BudgetWise.BLL.Services;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Moq;

namespace BudgetWise.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        [Fact]
        public async Task GetCategoriesAsync_ReturnsUserCategories_WhenRoleIsUser()
        {
            // Arrange
            var secondCategoryId = 2;
            var userId = 1;
            var categoryType = CategoryType.Expense;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            var queryReq = CreateQueryRequest();

            var categories = new List<Category>
            {
                CreateCategory(),
                CreateCategory(id: secondCategoryId, name: "Transportation"),
            };

            var mappedCategories = new List<CategoryResDto>
            {
                CreateMappedCategoryResponse(categories[0].Id, categories[0].UserId, categories[0].Name, categories[0].Type, categories[0].IsDeleted),
                CreateMappedCategoryResponse(categories[1].Id, categories[1].UserId, categories[1].Name, categories[1].Type, categories[1].IsDeleted)
            };

            var expectedResponse = (
                Data: categories,
                HasNextPage: false
            );

            mockCategoryRepo
                .Setup(x => x.GetCategoriesByUserAsync(userId, queryReq.Type, queryReq.Page, queryReq.Limit, queryReq.Search))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<CategoryResDto>>(It.IsAny<List<Category>>()))
                .Returns(mappedCategories);

            // Act
            var (data, _) = await categoryService.GetCategoriesAsync(userId, "User", queryReq);

            // Assert
            Assert.Equal(mappedCategories[0].Name, data[0].Name);
            Assert.Equal(CategoryType.Expense, data[1].Type);

            mockCategoryRepo.Verify(
                x => x.GetCategoriesByUserAsync(userId, categoryType, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsAllCategories_WhenRoleIsNotUser()
        {
            // Arrange
            var secondCategoryId = 2;
            var thirdCategoryId = 3;
            var firstUserId = 1;
            var secondUserId = 2;
            var categoryType = CategoryType.Expense;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            var queryReq = CreateQueryRequest();

            var categories = new List<Category>
            {
                CreateCategory(),
                CreateCategory(id: secondCategoryId, name: "Transportation", type: categoryType),
                CreateCategory(thirdCategoryId, secondUserId, "Internet", categoryType),
            };

            var mappedCategories = new List<CategoryResDto>
            {
                CreateMappedCategoryResponse(categories[0].Id, categories[0].UserId, categories[0].Name, categories[0].Type, categories[0].IsDeleted),
                CreateMappedCategoryResponse(categories[1].Id, categories[1].UserId, categories[1].Name, categories[1].Type, categories[1].IsDeleted),
                CreateMappedCategoryResponse(categories[2].Id, categories[2].UserId, categories[2].Name, categories[2].Type, categories[2].IsDeleted)
            };

            var expectedResponse = (
                Data: categories,
                HasNextPage: false
            );

            mockCategoryRepo
                .Setup(x => x.GetAllCategoriesAsync(queryReq.Page, queryReq.Limit, queryReq.Search))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<CategoryResDto>>(It.IsAny<List<Category>>()))
                .Returns(mappedCategories);

            // Act
            var (data, _) = await categoryService.GetCategoriesAsync(firstUserId, "SuperAdmin", queryReq);

            // Assert
            Assert.Equal(mappedCategories[0].Name, data[0].Name);
            Assert.Equal(mappedCategories[1].Type, data[1].Type);

            mockCategoryRepo.Verify(
                x => x.GetAllCategoriesAsync(1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsCategory_WhenFound()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var mappedCategory = CreateMappedCategoryResponse();

            mockMapper
                .Setup(x => x.Map<CategoryResDto>(It.IsAny<Category>()))
                .Returns(mappedCategory);

            mockCategoryRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await categoryService.GetCategoryByIdAsync(userId, "User", categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Type, result.Type);

            mockCategoryRepo.Verify(
                x => x.GetCategoryByUserAsync(userId, categoryId),
                Times.Once);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            mockCategoryRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await categoryService.GetCategoryByIdAsync(userId, "User", categoryId);

            // Assert
            Assert.Null(result);

            mockCategoryRepo.Verify(
                x => x.GetCategoryByUserAsync(userId, categoryId),
                Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_ReturnsTrue_WhenRequestIsValid()
        {
            // Arrange
            var userId = 1;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            var request = CreateCategoryRequest();
            var mappedCategory = CreateMappedCategoryResponse();

            mockMapper
                .Setup(x => x.Map<CategoryResDto>(It.IsAny<Category>()))
                .Returns(mappedCategory);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            // Act
            var result = await categoryService.CreateCategoryAsync(userId, request);

            // Assert
            Assert.True(result);

            mockCategoryRepo.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsUpdatedTrue_WhenCategoryExists()
        {
            // Arrange
            var userId = 1;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            var request = UpdateCategory();
            var existingCategory = CreateCategory(name: request.Name, type: request.Type!.Value);
            var mappedCategory = CreateMappedCategoryResponse();

            mockMapper
                .Setup(x => x.Map<CategoryResDto>(It.IsAny<Category>()))
                .Returns(mappedCategory);

            mockCategoryRepo.Setup(x => x.GetCategoryByUserAsync(userId, existingCategory.Id))
                .ReturnsAsync(existingCategory);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            var service = new CategoryService(mockCategoryRepo.Object, mockUserRepo.Object, mockMapper.Object);

            // Act
            var result = await service.UpdateCategoryAsync(userId, existingCategory.Id, request);

            // Assert
            Assert.True(result);

            mockCategoryRepo.Verify(
                x => x.GetCategoryByUserAsync(userId, existingCategory.Id),
                Times.Once);

            mockCategoryRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            var request = UpdateCategory();

            mockCategoryRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await categoryService.UpdateCategoryAsync(userId, categoryId, request);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsTrue_WhenExpenseExists()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            var existingCategory = CreateCategory();

            mockCategoryRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync(existingCategory);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            // Act
            var result = await categoryService.DeleteCategoryAsync(userId, categoryId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            var userId = 1;

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateCategoryService(mockCategoryRepo, mockUserRepo, mockMapper);

            mockCategoryRepo.Setup(x => x.GetCategoryByUserAsync(userId, categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await categoryService.DeleteCategoryAsync(userId, categoryId);

            // Assert
            Assert.False(result);
        }

        // Helper Functions
        private static CategoryService CreateCategoryService(
            Mock<ICategoryRepository>? mockCategoryRepo = null,
            Mock<IUserRepository>? mockUserRepo = null,
            Mock<IMapper>? mockMapper = null)
        {
            return new CategoryService(
                mockCategoryRepo!.Object,
                mockUserRepo!.Object,
                mockMapper!.Object);
        }

        private static Category CreateCategory(
            int id = 1,
            int userId = 1,
            string name = "Rent",
            CategoryType type = CategoryType.Expense,
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

        private static CategoryResDto CreateMappedCategoryResponse(
            int id = 1,
            int userId = 1,
            string name = "Rent",
            CategoryType type = CategoryType.Expense,
            bool isDeleted = false)
        {
            return new CategoryResDto
            {
                Id = id,
                UserId = userId,
                Name = name,
                Type = type,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
