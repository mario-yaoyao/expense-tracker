using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Org.BouncyCastle.Asn1.Ocsp;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class ExpenseServiceTests
    {
        [Fact]
        public async Task GetExpensesAsync_ReturnsUserExpenses_WhenRoleIsUser()
        {
            // Arrange
            var secondExpenseId = 2;
            var userId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var paginationReq = CreatePaginationRequest();

            var expenses = new List<Expense>
            {
                CreateExpense(category),
                CreateExpense(category, secondExpenseId),
            };

            var mappedExpenses = new List<ExpenseResDto>
            {
                CreateMappedExpense(expenses[0].Id, expenses[0].UserId, expenses[0].Description, expenses[0].Amount, expenses[0].Category.Name, expenses[0].Category.Type),
                CreateMappedExpense(expenses[1].Id, expenses[1].UserId, expenses[1].Description, expenses[1].Amount, expenses[1].Category.Name, expenses[1].Category.Type),
            };

            var expectedResponse = (
                Data: expenses,
                TotalExpense: 49.49m,
                HighestExpense: new HighestAmountResDto
                {
                    Name = "Expense 4",
                    Amount = 400m,
                },
                TotalCount: 2,
                HasNextPage: false
            );

            mockExpenseRepo
                .Setup(x => x.GetExpensesByUserAsync(userId, 1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<ExpenseResDto>>(It.IsAny<List<Expense>>()))
                .Returns(mappedExpenses);

            // Act
            var result = await expenseService.GetExpensesAsync(userId, "User", paginationReq);

            // Assert
            Assert.Equal(2, result.totalCount);
            Assert.Equal("Expense 1", result.data[0].Description);
            Assert.Equal("Expense 2", result.data[1].Description);

            mockExpenseRepo.Verify(
                x => x.GetExpensesByUserAsync(userId, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetExpensesAsync_ReturnsAllExpenses_WhenRoleIsSuperAdmin()
        {
            // Arrange
            var secondExpenseId = 2;
            var thirdExpenseId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            var paginationReq = CreatePaginationRequest();
            var category = CreateCategory();

            var expenses = new List<Expense>
            {
                CreateExpense(category),
                CreateExpense(category, secondExpenseId, firstUserId),
                CreateExpense(category, thirdExpenseId, secondUserId),
            };

            var mappedExpenses = new List<ExpenseResDto>
            {
                CreateMappedExpense(expenses[0].Id, expenses[0].UserId, expenses[0].Description, expenses[0].Amount, expenses[0].Category.Name, expenses[0].Category.Type),
                CreateMappedExpense(expenses[1].Id, expenses[1].UserId, expenses[1].Description, expenses[1].Amount, expenses[1].Category.Name, expenses[1].Category.Type),
                CreateMappedExpense(expenses[2].Id, expenses[2].UserId, expenses[2].Description, expenses[2].Amount, expenses[2].Category.Name, expenses[2].Category.Type),
            };

            var expectedResponse = (
                Data: expenses,
                TotalCount: 3,
                HasNextPage: false
            );

            mockExpenseRepo
                .Setup(x => x.GetAllExpensesAsync(1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<ExpenseResDto>>(It.IsAny<List<Expense>>()))
                .Returns(mappedExpenses);

            // Act
            var result = await expenseService.GetExpensesAsync(firstUserId, "SuperAdmin", paginationReq);

            // Assert
            Assert.Equal(expectedResponse.TotalCount, result.totalCount);
            Assert.Equal(mappedExpenses[0].Description, result.data[0].Description);
            Assert.Equal(mappedExpenses[1].Description, result.data[1].Description);
            Assert.Equal(mappedExpenses[2].Description, result.data[2].Description);

            mockExpenseRepo.Verify(
                x => x.GetAllExpensesAsync(1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsExpense_WhenFound()
        {
            // Arrange
            var expenseId = 1;
            var userId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var expense = CreateExpense(category);
            var mappedExpense = CreateMappedExpense(expense.Id, expense.UserId, expense.Description, expense.Amount, expense.Category.Name, expense.Category.Type);

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(mappedExpense);

            mockExpenseRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync(expense);

            // Act
            var result = await expenseService.GetExpenseByIdAsync(userId, "User", expenseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expenseId, result.Id);
            Assert.Equal(expense.Description, result.Description);
            Assert.Equal(expense.Amount, result.Amount);

            mockExpenseRepo.Verify(
                x => x.GetExpenseByUserAsync(userId, expenseId),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsNull_WhenExpenseDoesNotExist()
        {
            // Arrange
            var expenseId = 1;
            var userId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            mockExpenseRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await expenseService.GetExpenseByIdAsync(userId, "User", expenseId);

            // Assert
            Assert.Null(result);

            mockExpenseRepo.Verify(
                x => x.GetExpenseByUserAsync(userId, expenseId),
                Times.Once);
        }

        [Fact]
        public async Task CreateExpenseAsync_ReturnsTrue_WhenRequestIsValid()
        {
            // Arrange
            var userId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var expense = CreateExpenseRequest();
            var mappedExpense = CreateMappedExpense(description: expense.Description, amount: expense.Amount);

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(mappedExpense);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            // Act
            var result = await expenseService.CreateExpenseAsync(userId, expense);

            // Assert
            Assert.True(result);

            mockExpenseRepo.Verify(
                x => x.AddExpenseAsync(It.IsAny<Expense>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateExpenseAsync_ReturnsTrue_WhenExpenseExists()
        {
            // Arrange
            var secondExpenseId = 2;
            var userId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            var categories = new List<Category>
            {
                CreateCategory(),
                CreateCategory(id: secondExpenseId, name: "Grocery")
            };

            var request = UpdateExpense();
            var existingExpense = CreateExpense(categories[0]);
            var mappedExpense = CreateMappedExpense(
                description: request.Description,
                amount: request.Amount!.Value,
                categoryName: categories[1].Name,
                categoryType: categories[1].Type
            );

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(mappedExpense);

            mockExpenseRepo.Setup(x => x.GetExpenseByUserAsync(userId, existingExpense.Id))
                .ReturnsAsync(existingExpense);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            // Act
            var result = await expenseService.UpdateExpenseAsync(userId, existingExpense.Id, request);

            // Assert
            Assert.True(result);

            mockExpenseRepo.Verify(
                x => x.GetExpenseByUserAsync(userId, existingExpense.Id),
                Times.Once);

            mockExpenseRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateExpenseAsync_ReturnsFalse_WhenExpenseDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            var request = UpdateExpense();

            mockExpenseRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await expenseService.UpdateExpenseAsync(userId, expenseId, request);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsTrue_WhenExpenseExists()
        {
            // Arrange
            var userId = 1;
            var expenseId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var existingExpense = CreateExpense(category);

            mockExpenseRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync(existingExpense);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            // Act
            var result = await expenseService.DeleteExpenseAsync(userId, expenseId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsFalse_WhenExpenseDoesNotExist()
        {
            // Arrange
            var expenseId = 1;
            var userId = 1;

            var mockExpenseRepo = new Mock<IExpenseRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var expenseService = CreateCategoryService(mockExpenseRepo, mockUserRepo, mockMapper);

            mockExpenseRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await expenseService.DeleteExpenseAsync(userId, expenseId);

            // Assert
            Assert.False(result);
        }

        // Helper Functions
        private ExpenseService CreateCategoryService(
            Mock<IExpenseRepository>? mockExpenseRepo = null,
            Mock<IUserRepository>? mockUserRepo = null,
            Mock<IMapper>? mockMapper = null)
        {
            return new ExpenseService(
                mockExpenseRepo!.Object,
                mockUserRepo!.Object,
                mockMapper!.Object);
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
            Category category,
            int id = 1,
            int userId = 1,
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

        private static ExpenseResDto CreateMappedExpense(
            int id = 1,
            int userId = 1,
            string description = "Test Expense",
            decimal amount = 100,
            string categoryName = "Transportation",
            CategoryType categoryType = CategoryType.Expense,
            DateTime? createdAt = null,
            DateTime? updatedAt = null)
        {
            return new ExpenseResDto
            {
                Id = id,
                UserId = userId,
                Description = description,
                Amount = amount,
                CategoryName = categoryName,
                CategoryType = categoryType,
                CreatedAt = createdAt ?? new DateTime(2026, 1, 1),
                UpdatedAt = updatedAt ?? new DateTime(2026, 1, 2)
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
