using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class ExpenseServiceTests
    {
        private readonly Mock<IMapper> mockMapper;

        public ExpenseServiceTests()
        {
            mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetExpensesAsync_ReturnsUserExpenses_WhenRoleIsUser()
        {
            // Arrange
            var firstExpenseId = 1;
            var secondExpenseId = 2;
            var userId = 1;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();

            var request = new List<Expense>
            {
                CreateExpense(firstExpenseId, userId, category),
                CreateExpense(secondExpenseId, userId, category),
            };
            var paginationReq = CreatePaginationRequest();

            var expectedResponseData = new List<ExpenseResDto>
            {
                new()
                {
                    Id = request[0].Id,
                    UserId = request[0].UserId,
                    Description = request[0].Description,
                    Amount = request[0].Amount,
                    CategoryName = request[0].Category.Name,
                    CategoryType = request[0].Category.Type
                },
                new()
                {
                    Id = request[1].Id,
                    UserId = request[1].UserId,
                    Description = request[1].Description,
                    Amount = request[1].Amount,
                    CategoryName = request[1].Category.Name,
                    CategoryType = request[1].Category.Type
                }
            };

            var expectedResponse = (
                Data: request,
                TotalExpense: 49.49m,
                HighestExpense: new HighestAmountResDto
                {
                    Name = "Expense 4",
                    Amount = 400m,
                },
                TotalCount: 2,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetExpensesByUserAsync(userId, 1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<ExpenseResDto>>(It.IsAny<List<Expense>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetExpensesAsync(userId, "User", paginationReq);

            // Assert
            Assert.Equal(2, result.totalCount);
            Assert.Equal("Expense 1", result.data[0].Description);
            Assert.Equal("Expense 2", result.data[1].Description);

            mockRepo.Verify(
                x => x.GetExpensesByUserAsync(userId, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetExpensesAsync_ReturnsAllExpenses_WhenRoleIsNotUser()
        {
            // Arrange
            var firstExpenseId = 1;
            var secondExpenseId = 2;
            var thirdExpenseId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();
            var paginationReq = CreatePaginationRequest();

            var request = new List<Expense>
            {
                CreateExpense(firstExpenseId, firstUserId, category),
                CreateExpense(secondExpenseId, firstUserId, category),
                CreateExpense(thirdExpenseId, secondUserId, category),
            };

            var expectedResponseData = new List<ExpenseResDto>
            {
                new()
                {
                    Id = request[0].Id,
                    UserId = request[0].UserId,
                    Description = request[0].Description,
                    Amount = request[0].Amount,
                    CategoryName = request[0].Category.Name,
                    CategoryType = request[0].Category.Type
                },
                new()
                {
                    Id = request[1].Id,
                    UserId = request[1].UserId,
                    Description = request[1].Description,
                    Amount = request[1].Amount,
                    CategoryName = request[1].Category.Name,
                    CategoryType = request[1].Category.Type
                },
                new()
                {
                    Id = request[2].Id,
                    UserId = request[2].UserId,
                    Description = request[2].Description,
                    Amount = request[2].Amount,
                }
            };

            var expectedResponse = (
                Data: request,
                TotalCount: 3,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetAllExpensesAsync(1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<ExpenseResDto>>(It.IsAny<List<Expense>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetExpensesAsync(firstUserId, "SuperAdmin", paginationReq);

            // Assert
            Assert.Equal(expectedResponse.TotalCount, result.totalCount);
            Assert.Equal(expectedResponseData[0].Description, result.data[0].Description);
            Assert.Equal(expectedResponseData[1].Description, result.data[1].Description);
            Assert.Equal(expectedResponseData[2].Description, result.data[2].Description);

            mockRepo.Verify(
                x => x.GetAllExpensesAsync(1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsExpense_WhenFound()
        {
            // Arrange
            var expenseId = 1;
            var userId = 1;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();

            var expectedResponse = new Expense
            {
                Id = expenseId,
                UserId = userId,
                Description = "Expense 1",
                Amount = 100,
                CategoryId = category.Id,
                Category = category,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var expectedResDto = new ExpenseResDto
            {
                Id = expectedResponse.Id,
                UserId = expectedResponse.UserId,
                Description = expectedResponse.Description,
                Amount = expectedResponse.Amount,
                CategoryName = expectedResponse.Category.Name,
                CategoryType = expectedResponse.Category.Type,
                CreatedAt = expectedResponse.CreatedAt,
                UpdatedAt = expectedResponse.UpdatedAt
            };

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(expectedResDto);

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await service.GetExpenseByIdAsync(userId, "User", expenseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expenseId, result.Id);
            Assert.Equal(expectedResponse.Description, result.Description);
            Assert.Equal(expectedResponse.Amount, result.Amount);

            mockRepo.Verify(
                x => x.GetExpenseByUserAsync(userId, expenseId),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var expenseId = 1;
            var userId = 1;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await service.GetExpenseByIdAsync(userId, "User", expenseId);

            // Assert
            Assert.Null(result);

            mockRepo.Verify(
                x => x.GetExpenseByUserAsync(userId, expenseId),
                Times.Once);
        }

        [Fact]
        public async Task CreateExpenseAsync_ReturnsCreatedExpense_WhenRequestIsValid()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();
            var request = CreateExpenseRequest();

            var expectedResponseDto = new ExpenseResDto
            {
                Id = expenseId,
                UserId = userId,
                Description = request.Description,
                Amount = request.Amount,
                CategoryName = category.Name,
                CategoryType = category.Type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(expectedResponseDto);

            // Act
            var result = await service.CreateExpenseAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(category.Name, result.CategoryName);
            Assert.Equal(category.Type, result.CategoryType);

            mockRepo.Verify(
                x => x.AddExpenseAsync(It.IsAny<Expense>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateExpenseAsync_ReturnsUpdatedExpense_WhenExpenseExists()
        {
            // Arrange
            var firstExpenseId = 1;
            var userId = 1;

            var mockRepo = new Mock<IExpenseRepository>();

            var firstCategory = CreateCategory();
            var secondCategory = CreateCategory(id: 2, name: "Grocery");
            var request = UpdateExpense();

            var existingExpense = new Expense
            {
                Id = firstExpenseId,
                UserId = userId,
                Description = "Expense 4",
                Amount = 400,
                CategoryId = firstCategory.Id,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var expectedResDto = new ExpenseResDto
            {
                Id = firstExpenseId,
                UserId = userId,
                Description = request.Description,
                Amount = (decimal)request.Amount!,
                CategoryName = secondCategory.Name,
                CategoryType = secondCategory.Type,
                CreatedAt = existingExpense.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(expectedResDto);

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, existingExpense.Id))
                .ReturnsAsync(existingExpense);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.UpdateExpenseAsync(userId, existingExpense.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UpdatedAt);
            Assert.True(result.UpdatedAt > result.CreatedAt);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(secondCategory.Name, result.CategoryName);
            Assert.Equal(secondCategory.Type, result.CategoryType);

            mockRepo.Verify(
                x => x.GetExpenseByUserAsync(userId, existingExpense.Id),
                Times.Once);

            mockRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateExpenseAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            var request = UpdateExpense();

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await service.UpdateExpenseAsync(userId, expenseId, request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsTrue_WhenExpenseExists()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();

            var existingExpense = new Expense
            {
                Id = expenseId,
                UserId = userId,
                Description = "Expense 1",
                Amount = 100,
                CategoryId = category.Id,
                IsDeleted = false
            };

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync(existingExpense);

            // Act
            var result = await service.DeleteExpenseAsync(userId, expenseId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsFalse_WhenExpenseDoesNotExist()
        {
            // Arrange
            var expenseId = 1;
            var userId = 1;

            var mockRepo = new Mock<IExpenseRepository>();
            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await service.DeleteExpenseAsync(userId, expenseId);

            // Assert
            Assert.False(result);
        }

        // Helper Functions
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

        private static UpdateExpenseReqDto UpdateExpense(
            string description = "Updated Expense",
            decimal amount = 450m,
            int categoryId = 1)
        {
            return new UpdateExpenseReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId,
            };
        }

        private static CreateExpenseReqDto CreateExpenseRequest(
            string description = "Expense 4",
            decimal amount = 400m,
            int categoryId = 1)
        {
            return new CreateExpenseReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId
            };
        }

        private static ExpenseQueryReqDto CreatePaginationRequest(
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new ExpenseQueryReqDto
            {
                Page = page,
                Limit = limit,
                Search = search
            };
        }
    }
}
