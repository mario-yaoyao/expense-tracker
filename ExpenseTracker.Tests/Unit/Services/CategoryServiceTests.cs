using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

//using ExpenseTracker.Models.Dtos.Requests;
//using ExpenseTracker.Models.Dtos.Responses;
//using ExpenseTracker.Models.Models;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        //TODO:

        //GetCategoriesAsync

        //GetCategoryByIdAsync

        //CreateCategoryAsync

        //UpdateCategoryAsync

        //DeleteCategoryAsync

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
                TotalCount: 2,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetCategoriesByUserAsync(userId, categoryType, 1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<CategoryResDto>>(It.IsAny<List<Category>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetCategoriesAsync(userId, "User", categoryType);

            // Assert
            Assert.Equal(2, result.totalCount);
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
                TotalCount: 3,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetAllCategoriesAsync(1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<CategoryResDto>>(It.IsAny<List<Category>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetCategoriesAsync(firstUserId, "SuperAdmin");

            // Assert
            Assert.Equal(expectedResponse.TotalCount, result.totalCount);
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
            var categoryType = CategoryType.Expense;

            var mockRepo = new Mock<ICategoryRepository>();
            var service = new CategoryService(mockRepo.Object, mockMapper.Object);

            var request = CreateCategory(categoryId, userId, "Grocery", categoryType);

            var expectedResponse = new CategoryResDto
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
                .Returns(expectedResponse);

            // Act
            var result = await service.CreateCategoryAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(request.Name, result.Description);
            Assert.Equal(request.Type, result.Type);

            mockRepo.Verify(
                x => x.AddCategoryAsync(It.IsAny<Category>()),
                Times.Once);
        }

        //[Fact]
        //public async Task UpdateExpenseAsync_ReturnsUpdatedExpense_WhenExpenseExists()
        //{
        //    // Arrange
        //    var firstExpenseId = 1;
        //    var userId = 1;

        //    var mockRepo = new Mock<IExpenseRepository>();

        //    var firstCategory = CreateCategory();
        //    var secondCategory = CreateCategory(id: 2, name: "Grocery");
        //    var request = UpdateExpense();

        //    var existingExpense = new Expense
        //    {
        //        Id = firstExpenseId,
        //        UserId = userId,
        //        Description = "Expense 4",
        //        Amount = 400,
        //        CategoryId = firstCategory.Id,
        //        IsDeleted = false,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = null
        //    };

        //    var expectedResDto = new ExpenseResDto
        //    {
        //        Id = firstExpenseId,
        //        UserId = userId,
        //        Description = request.Description,
        //        Amount = (decimal)request.Amount!,
        //        CategoryName = secondCategory.Name,
        //        CategoryType = secondCategory.Type,
        //        CreatedAt = existingExpense.CreatedAt,
        //        UpdatedAt = DateTime.UtcNow
        //    };

        //    mockMapper
        //        .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
        //        .Returns(expectedResDto);

        //    mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, existingExpense.Id))
        //        .ReturnsAsync(existingExpense);

        //    var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

        //    // Act
        //    var result = await service.UpdateExpenseAsync(userId, existingExpense.Id, request);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.NotNull(result.UpdatedAt);
        //    Assert.True(result.UpdatedAt > result.CreatedAt);
        //    Assert.Equal(userId, result.UserId);
        //    Assert.Equal(request.Description, result.Description);
        //    Assert.Equal(request.Amount, result.Amount);
        //    Assert.Equal(secondCategory.Name, result.CategoryName);
        //    Assert.Equal(secondCategory.Type, result.CategoryType);

        //    mockRepo.Verify(
        //        x => x.GetExpenseByUserAsync(userId, existingExpense.Id),
        //        Times.Once);

        //    mockRepo.Verify(
        //        x => x.SaveChangesAsync(),
        //        Times.Once);
        //}

        //[Fact]
        //public async Task UpdateExpenseAsync_ReturnsNull_WhenExpenseDoesNotExist()
        //{
        //    // Arrange
        //    var userId = 1;
        //    var expenseId = 1;

        //    var mockRepo = new Mock<IExpenseRepository>();
        //    var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

        //    var request = UpdateExpense();

        //    mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
        //        .ReturnsAsync((Expense?)null);

        //    // Act
        //    var result = await service.UpdateExpenseAsync(userId, expenseId, request);

        //    // Assert
        //    Assert.Null(result);
        //}

        //[Fact]
        //public async Task DeleteExpenseAsync_ReturnsTrue_WhenExpenseExists()
        //{
        //    // Arrange
        //    var userId = 1;
        //    var expenseId = 1;

        //    var mockRepo = new Mock<IExpenseRepository>();
        //    var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

        //    var category = CreateCategory();

        //    var existingExpense = new Expense
        //    {
        //        Id = expenseId,
        //        UserId = userId,
        //        Description = "Expense 1",
        //        Amount = 100,
        //        CategoryId = category.Id,
        //        IsDeleted = false
        //    };

        //    mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
        //        .ReturnsAsync(existingExpense);

        //    // Act
        //    var result = await service.DeleteExpenseAsync(userId, expenseId);

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public async Task DeleteExpenseAsync_ReturnsFalse_WhenExpenseDoesNotExist()
        //{
        //    // Arrange
        //    var expenseId = 1;
        //    var userId = 1;

        //    var mockRepo = new Mock<IExpenseRepository>();
        //    var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

        //    mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
        //        .ReturnsAsync((Expense?)null);

        //    // Act
        //    var result = await service.DeleteExpenseAsync(userId, expenseId);

        //    // Assert
        //    Assert.False(result);
        //}

        // Helper Functions
        //private static Category CreateCategory(
        //    int id = 1,
        //    string name = "Transportation")
        //{
        //    return new Category
        //    {
        //        Id = id,
        //        Name = name
        //    };
        //}

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
            CategoryType type = 0)
        {
            return new UpdateCategoryReqDto
            {
                Name = name,
                Type = type
            };
        }

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
    }
}
