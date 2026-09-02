using AutoMapper;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services
{
    public class IncomeServiceTests
    {
        [Fact]
        public async Task GetIncomesAsync_ReturnsUserIncomes_WhenRoleIsUser()
        {
            // Arrange
            var firstIncomeId = 1;
            var secondIncomeId = 2;
            var userId = 1;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();

            var request = new List<Income>
            {
                CreateIncome(firstIncomeId, userId, category),
                CreateIncome(secondIncomeId, userId, category),
            };
            var paginationReq = CreatePaginationRequest();

            var expectedResponseData = new List<IncomeResDto>
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
                    Name = "Income 4",
                    Amount = 400m,
                },
                TotalCount: 2,
                HasNextPage: false
            );

            mockRepo
                .Setup(x => x.GetIncomesByUserAsync(userId, 1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<IncomeResDto>>(It.IsAny<List<Income>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetIncomesAsync(userId, "User", paginationReq);

            // Assert
            Assert.Equal(2, result.totalCount);
            Assert.Equal("Income 1", result.data[0].Description);
            Assert.Equal("Income 2", result.data[1].Description);

            mockRepo.Verify(
                x => x.GetIncomesByUserAsync(userId, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetIncomesAsync_ReturnsAllIncomes_WhenRoleIsNotUser()
        {
            // Arrange
            var firstIncomeId = 1;
            var secondIncomeId = 2;
            var thirdIncomeId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();
            var paginationReq = CreatePaginationRequest();

            var request = new List<Income>
            {
                CreateIncome(firstIncomeId, firstUserId, category),
                CreateIncome(secondIncomeId, firstUserId, category),
                CreateIncome(thirdIncomeId, secondUserId, category),
            };

            var expectedResponseData = new List<IncomeResDto>
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
                .Setup(x => x.GetAllIncomesAsync(1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<IncomeResDto>>(It.IsAny<List<Income>>()))
                .Returns(expectedResponseData);

            // Act
            var result = await service.GetIncomesAsync(firstUserId, "SuperAdmin", paginationReq);

            // Assert
            Assert.Equal(expectedResponse.TotalCount, result.totalCount);
            Assert.Equal(expectedResponseData[0].Description, result.data[0].Description);
            Assert.Equal(expectedResponseData[1].Description, result.data[1].Description);
            Assert.Equal(expectedResponseData[2].Description, result.data[2].Description);

            mockRepo.Verify(
                x => x.GetAllIncomesAsync(1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetIncomeByIdAsync_ReturnsIncome_WhenFound()
        {
            // Arrange
            var incomeId = 1;
            var userId = 1;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();

            var expectedResponse = new Income
            {
                Id = incomeId,
                UserId = userId,
                Description = "Income 1",
                Amount = 100,
                CategoryId = category.Id,
                Category = category,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var expectedResDto = new IncomeResDto
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
                .Setup(x => x.Map<IncomeResDto>(It.IsAny<Income>()))
                .Returns(expectedResDto);

            mockRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await service.GetIncomeByIdAsync(userId, "User", incomeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(incomeId, result.Id);
            Assert.Equal(expectedResponse.Description, result.Description);
            Assert.Equal(expectedResponse.Amount, result.Amount);

            mockRepo.Verify(
                x => x.GetIncomeByUserAsync(userId, incomeId),
                Times.Once);
        }

        [Fact]
        public async Task GetIncomeByIdAsync_ReturnsNull_WhenIncomeDoesNotExist()
        {
            // Arrange
            var incomeId = 1;
            var userId = 1;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync((Income?)null);

            // Act
            var result = await service.GetIncomeByIdAsync(userId, "User", incomeId);

            // Assert
            Assert.Null(result);

            mockRepo.Verify(
                x => x.GetIncomeByUserAsync(userId, incomeId),
                Times.Once);
        }

        [Fact]
        public async Task CreateIncomeAsync_ReturnsCreatedIncome_WhenRequestIsValid()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();
            var request = CreateIncomeRequest();

            var expectedResponseDto = new IncomeResDto
            {
                Id = incomeId,
                UserId = userId,
                Description = request.Description,
                Amount = request.Amount,
                CategoryName = category.Name,
                CategoryType = category.Type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockMapper
                .Setup(x => x.Map<IncomeResDto>(It.IsAny<Income>()))
                .Returns(expectedResponseDto);

            // Act
            var result = await service.CreateIncomeAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Amount, result.Amount);
            Assert.Equal(category.Name, result.CategoryName);
            Assert.Equal(category.Type, result.CategoryType);

            mockRepo.Verify(
                x => x.AddIncomeAsync(It.IsAny<Income>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateIncomeAsync_ReturnsUpdatedIncome_WhenIncomeExists()
        {
            // Arrange
            var firstIncomeId = 1;
            var userId = 1;

            var mockRepo = new Mock<IIncomeRepository>();

            var firstCategory = CreateCategory();
            var secondCategory = CreateCategory(id: 2, name: "Bonus");
            var request = UpdateIncome();

            var existingIncome = new Income
            {
                Id = firstIncomeId,
                UserId = userId,
                Description = "Income 4",
                Amount = 400,
                CategoryId = firstCategory.Id,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var expectedResDto = new IncomeResDto
            {
                Id = firstIncomeId,
                UserId = userId,
                Description = request.Description,
                Amount = (decimal)request.Amount!,
                CategoryName = secondCategory.Name,
                CategoryType = secondCategory.Type,
                CreatedAt = existingIncome.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            mockMapper
                .Setup(x => x.Map<IncomeResDto>(It.IsAny<Income>()))
                .Returns(expectedResDto);

            mockRepo.Setup(x => x.GetIncomeByUserAsync(userId, existingIncome.Id))
                .ReturnsAsync(existingIncome);

            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            // Act
            var result = await service.UpdateIncomeAsync(userId, existingIncome.Id, request);

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
                x => x.GetIncomeByUserAsync(userId, existingIncome.Id),
                Times.Once);

            mockRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateIncomeAsync_ReturnsNull_WhenIncomeDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            var request = UpdateIncome();

            mockRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync((Income?)null);

            // Act
            var result = await service.UpdateIncomeAsync(userId, incomeId, request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteIncomeAsync_ReturnsTrue_WhenIncomeExists()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            var category = CreateCategory();

            var existingIncome = new Income
            {
                Id = incomeId,
                UserId = userId,
                Description = "Income 1",
                Amount = 100,
                CategoryId = category.Id,
                IsDeleted = false
            };

            mockRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync(existingIncome);

            // Act
            var result = await service.DeleteIncomeAsync(userId, incomeId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteIncomeAsync_ReturnsFalse_WhenIncomeDoesNotExist()
        {
            // Arrange
            var incomeId = 1;
            var userId = 1;

            var mockRepo = new Mock<IIncomeRepository>();
            var service = new IncomeService(mockRepo.Object, mockMapper.Object);

            mockRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync((Income?)null);

            // Act
            var result = await service.DeleteIncomeAsync(userId, incomeId);

            // Assert
            Assert.False(result);
        }

        // Helper Functions
        private readonly Mock<IMapper> mockMapper;

        public IncomeServiceTests()
        {
            mockMapper = new Mock<IMapper>();
        }

        private static Category CreateCategory(
            int id = 1,
            string name = "Salary")
        {
            return new Category
            {
                Id = id,
                Name = name
            };
        }

        private static Income CreateIncome(
            int id,
            int userId,
            Category category,
            bool isDeleted = false)
        {
            return new Income
            {
                Id = id,
                UserId = userId,
                Description = $"Income {id}",
                Amount = 50m,
                CategoryId = category.Id,
                Category = category,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }

        private static UpdateIncomeReqDto UpdateIncome(
            string description = "Updated Income",
            decimal amount = 450m,
            int categoryId = 1)
        {
            return new UpdateIncomeReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId,
            };
        }

        private static CreateIncomeReqDto CreateIncomeRequest(
            string description = "Income 4",
            decimal amount = 400m,
            int categoryId = 1)
        {
            return new CreateIncomeReqDto
            {
                Description = description,
                Amount = amount,
                CategoryId = categoryId
            };
        }

        private static IncomeQueryReqDto CreatePaginationRequest(
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new IncomeQueryReqDto
            {
                Page = page,
                Limit = limit,
                Search = search
            };
        }
    }
}
