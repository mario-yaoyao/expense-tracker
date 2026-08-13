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
            var mockRepo = new Mock<IExpenseRepository>();
            var userId = Guid.NewGuid();

            var request = new List<Expense>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Description = "Expense 1",
                    Amount = 100,
                    Category = "Category 1",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Description = "Expense 2",
                    Amount = 200,
                    Category = "Category 2",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                }
            };

            var expectedResponse = new List<ExpenseResDto>
            {
                new()
                {
                    Id = request[0].Id,
                    UserId = request[0].UserId,
                    Description = request[0].Description,
                    Amount = request[0].Amount,
                    Category = request[0].Category
                },
                new()
                {
                    Id = request[1].Id,
                    UserId = request[1].UserId,
                    Description = request[1].Description,
                    Amount = request[1].Amount,
                    Category = request[1].Category
                }
            };

            mockRepo
                .Setup(x => x.GetExpensesByUserAsync(userId))
                .ReturnsAsync(request);

            mockMapper
                .Setup(x => x.Map<List<ExpenseResDto>>(It.IsAny<List<Expense>>()))
                .Returns(expectedResponse);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.GetExpensesAsync(userId, "User");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Expense 1", result[0].Description);
            Assert.Equal("Expense 2", result[1].Description);

            mockRepo.Verify(
                x => x.GetExpensesByUserAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task GetExpensesAsync_ReturnsAllExpenses_WhenRoleIsNotUser()
        {
            // Arrange
            var mockRepo = new Mock<IExpenseRepository>();
            var userId = Guid.NewGuid();

            var request = new List<Expense>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Description = "Expense 1",
                    Amount = 100,
                    Category = "Category 1",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Description = "Expense 2",
                    Amount = 200,
                    Category = "Category 2",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                },
                new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Description = "Expense 3",
                    Amount = 300,
                    Category = "Category 3",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                }
            };

            var expectedResponse = new List<ExpenseResDto>
            {
                new()
                {
                    Id = request[0].Id,
                    UserId = request[0].UserId,
                    Description = request[0].Description,
                    Amount = request[0].Amount,
                    Category = request[0].Category
                },
                new()
                {
                    Id = request[1].Id,
                    UserId = request[1].UserId,
                    Description = request[1].Description,
                    Amount = request[1].Amount,
                    Category = request[1].Category
                },
                new()
                {
                    Id = request[2].Id,
                    UserId = request[2].UserId,
                    Description = request[2].Description,
                    Amount = request[2].Amount,
                    Category = request[2].Category
                }
            };

            mockRepo
                .Setup(x => x.GetExpensesByUserAsync(userId))
                .ReturnsAsync(request);

            mockMapper
                .Setup(x => x.Map<List<ExpenseResDto>>(It.IsAny<List<Expense>>()))
                .Returns(expectedResponse);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.GetExpensesAsync(userId, "SuperAdmin");

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(expectedResponse[0].Description, result[0].Description);
            Assert.Equal(expectedResponse[1].Description, result[1].Description);
            Assert.Equal(expectedResponse[2].Description, result[2].Description);

            mockRepo.Verify(
                x => x.GetAllExpensesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ReturnsExpense_WhenFound()
        {
            // Arrange
            var mockRepo = new Mock<IExpenseRepository>();
            var expenseId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var expectedResponse = new Expense
            {
                Id = expenseId,
                UserId = userId,
                Description = "Expense 1",
                Amount = 100,
                Category = "Category 1",
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
                Category = expectedResponse.Category,
                CreatedAt = expectedResponse.CreatedAt,
                UpdatedAt = expectedResponse.UpdatedAt
            };

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(expectedResDto);

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync(expectedResponse);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

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
            var mockRepo = new Mock<IExpenseRepository>();
            var expenseId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

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
            var mockRepo = new Mock<IExpenseRepository>();
            var userId = Guid.NewGuid();

            var request = new CreateExpenseReqDto
            {
                Description = "Expense 4",
                Amount = 400,
                Category = "Category 4"
            };

            var expectedResDto = new ExpenseResDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Description = request.Description,
                Amount = request.Amount,
                Category = request.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockMapper
                .Setup(x => x.Map<ExpenseResDto>(It.IsAny<Expense>()))
                .Returns(expectedResDto);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.CreateExpenseAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(request.Category, result.Category);

            mockRepo.Verify(
                x => x.AddExpenseAsync(It.IsAny<Expense>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateExpenseAsync_ReturnsUpdatedExpense_WhenExpenseExists()
        {
            // Arrange
            var mockRepo = new Mock<IExpenseRepository>();
            var userId = Guid.NewGuid();

            var existingExpense = new Expense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Description = "Expense 4",
                Amount = 400,
                Category = "Category 4",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var request = new UpdateExpenseReqDto
            {
                Description = "Updated Expense 4",
                Amount = 450,
                Category = "Updated Category 4"
            };

            var expectedResDto = new ExpenseResDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Description = request.Description,
                Amount = request.Amount,
                Category = request.Category,
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
            var result = await service.UpdateExpenseAsync(userId, "User", existingExpense.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.UpdatedAt);
            Assert.True(result.UpdatedAt > result.CreatedAt);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(request.Category, result.Category);

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
            var mockRepo = new Mock<IExpenseRepository>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var request = new UpdateExpenseReqDto
            {
                Description = "Updated Expense 4",
                Amount = 450,
                Category = "Updated Category 4"
            };

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.UpdateExpenseAsync(userId, "User", expenseId, request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsTrue_WhenExpenseExists()
        {
            // Arrange
            var mockRepo = new Mock<IExpenseRepository>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            var existingExpense = new Expense
            {
                Id = expenseId,
                UserId = userId,
                Description = "Expense 1",
                Amount = 100,
                Category = "Category 1",
                IsDeleted = false
            };

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync(existingExpense);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.DeleteExpenseAsync(userId, "User", expenseId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteExpenseAsync_ReturnsFalse_WhenExpenseDoesNotExist()
        {
            // Arrange
            var mockRepo = new Mock<IExpenseRepository>();
            var userId = Guid.NewGuid();
            var expenseId = Guid.NewGuid();

            mockRepo.Setup(x => x.GetExpenseByUserAsync(userId, expenseId))
                .ReturnsAsync((Expense?)null);

            var service = new ExpenseService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.DeleteExpenseAsync(userId, "User", expenseId);

            // Assert
            Assert.False(result);
        }
    }
}
