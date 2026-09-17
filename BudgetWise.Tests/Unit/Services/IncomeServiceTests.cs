using AutoMapper;
using BudgetWise.BLL.Services;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Moq;

namespace BudgetWise.Tests.Unit.Services
{
    public class IncomeServiceTests
    {
        [Fact]
        public async Task GetIncomesAsync_ReturnsUserIncomes_WhenRoleIsUser()
        {
            // Arrange
            var secondIncomeId = 2;
            var userId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            var paginationReq = CreatePaginationRequest();
            var category = CreateCategory();

            var request = new List<Income>
            {
                CreateIncome(category),
                CreateIncome(category, id: secondIncomeId),
            };

            var mappedIncomes = new List<IncomeResDto>
            {
                CreateIncomeResponse(
                    id: request[0].Id,
                    userId: request[0].UserId,
                    description: request[0].Description,
                    amount: request[0].Amount,
                    categoryName: request[0].Category.Name,
                    categoryType: request[0].Category.Type),
                CreateIncomeResponse(
                    id: request[1].Id,
                    userId: request[1].UserId,
                    description: request[1].Description,
                    amount: request[1].Amount,
                    categoryName: request[1].Category.Name,
                    categoryType: request[1].Category.Type)
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

            mockIncomeRepo
                .Setup(x => x.GetIncomesByUserAsync(userId, 1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<IncomeResDto>>(It.IsAny<List<Income>>()))
                .Returns(mappedIncomes);

            // Act
            var (data, _, _, totalCount, _) = await incomeService.GetIncomesAsync(userId, "User", paginationReq);

            // Assert
            Assert.Equal(2, totalCount);
            Assert.Equal("Income 1", data[0].Description);
            Assert.Equal("Income 2", data[1].Description);

            mockIncomeRepo.Verify(
                x => x.GetIncomesByUserAsync(userId, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetIncomesAsync_ReturnsAllIncomes_WhenRoleIsNotUser()
        {
            // Arrange
            var secondIncomeId = 2;
            var thirdIncomeId = 3;
            var firstUserId = 1;
            var secondUserId = 2;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            var paginationReq = CreatePaginationRequest();
            var category = CreateCategory();

            var request = new List<Income>
            {
                CreateIncome(category),
                CreateIncome(category, secondIncomeId),
                CreateIncome(category, thirdIncomeId, secondUserId),
            };

            var mappedIncomes = new List<IncomeResDto>
            {
                CreateIncomeResponse(
                    id: request[0].Id,
                    userId: request[0].UserId,
                    description: request[0].Description,
                    amount: request[0].Amount,
                    categoryName: request[0].Category.Name,
                    categoryType: request[0].Category.Type),
                CreateIncomeResponse(
                    id: request[1].Id,
                    userId: request[1].UserId,
                    description: request[1].Description,
                    amount: request[1].Amount,
                    categoryName: request[1].Category.Name,
                    categoryType: request[1].Category.Type),
                CreateIncomeResponse(
                    id: request[2].Id,
                    userId: request[2].UserId,
                    description: request[2].Description,
                    amount: request[2].Amount)
            };

            var expectedResponse = (
                Data: request,
                TotalCount: 3,
                HasNextPage: false
            );

            mockIncomeRepo
                .Setup(x => x.GetAllIncomesAsync(1, 20, null))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<IncomeResDto>>(It.IsAny<List<Income>>()))
                .Returns(mappedIncomes);

            // Act
            var (data, _, _, totalCount, _) = await incomeService.GetIncomesAsync(firstUserId, "SuperAdmin", paginationReq);

            // Assert
            Assert.Equal(expectedResponse.TotalCount, totalCount);
            Assert.Equal(mappedIncomes[0].Description, data[0].Description);
            Assert.Equal(mappedIncomes[1].Description, data[1].Description);
            Assert.Equal(mappedIncomes[2].Description, data[2].Description);

            mockIncomeRepo.Verify(
                x => x.GetAllIncomesAsync(1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetIncomeByIdAsync_ReturnsIncome_WhenFound()
        {
            // Arrange
            var incomeId = 1;
            var userId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var request = CreateIncome(category, id: incomeId, userId: userId);
            var mappedIncome = CreateIncomeResponse(
                description: request.Description!,
                amount: request.Amount,
                categoryName: category.Name,
                categoryType: category.Type,
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.UtcNow);

            mockMapper
                .Setup(x => x.Map<IncomeResDto>(It.IsAny<Income>()))
                .Returns(mappedIncome);

            mockIncomeRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync(request);

            // Act
            var result = await incomeService.GetIncomeByIdAsync(userId, "User", incomeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(incomeId, result.Id);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Amount, result.Amount);

            mockIncomeRepo.Verify(
                x => x.GetIncomeByUserAsync(userId, incomeId),
                Times.Once);
        }

        [Fact]
        public async Task GetIncomeByIdAsync_ReturnsNull_WhenIncomeDoesNotExist()
        {
            // Arrange
            var incomeId = 1;
            var userId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            mockIncomeRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync((Income?)null);

            // Act
            var result = await incomeService.GetIncomeByIdAsync(userId, "User", incomeId);

            // Assert
            Assert.Null(result);

            mockIncomeRepo.Verify(
                x => x.GetIncomeByUserAsync(userId, incomeId),
                Times.Once);
        }

        [Fact]
        public async Task CreateIncomeAsync_ReturnsTrue_WhenRequestIsValid()
        {
            // Arrange
            var userId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var request = CreateIncomeRequest();
            var mappedIncome = CreateIncomeResponse(
                description: request.Description!,
                amount: request.Amount,
                categoryName: category.Name,
                categoryType: category.Type,
                createdAt: DateTime.UtcNow,
                updatedAt: DateTime.UtcNow);

            mockMapper
                .Setup(x => x.Map<IncomeResDto>(It.IsAny<Income>()))
                .Returns(mappedIncome);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            // Act
            var result = await incomeService.CreateIncomeAsync(userId, request);

            // Assert
            Assert.True(result);

            mockIncomeRepo.Verify(
                x => x.AddIncomeAsync(It.IsAny<Income>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateIncomeAsync_ReturnsTrue_WhenIncomeExists()
        {
            // Arrange
            var userId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            var categories = new List<Category>
            {
                CreateCategory(),
                CreateCategory(id: 2, name: "Bonus")
            };

            var request = UpdateIncome();
            var existingIncome = CreateIncome(categories[0]);
            var mappedIncome = CreateIncomeResponse(
                description: request.Description!,
                amount: request.Amount!.Value,
                categoryName: categories[1].Name,
                categoryType: categories[1].Type,
                createdAt: existingIncome.CreatedAt,
                updatedAt: DateTime.UtcNow);

            mockMapper
                .Setup(x => x.Map<IncomeResDto>(It.IsAny<Income>()))
                .Returns(mappedIncome);

            mockIncomeRepo.Setup(x => x.GetIncomeByUserAsync(userId, existingIncome.Id))
                .ReturnsAsync(existingIncome);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            var service = new IncomeService(mockIncomeRepo.Object, mockUserRepo.Object, mockMapper.Object);

            // Act
            var result = await service.UpdateIncomeAsync(userId, existingIncome.Id, request);

            // Assert
            Assert.True(result);

            mockIncomeRepo.Verify(
                x => x.GetIncomeByUserAsync(userId, existingIncome.Id),
                Times.Once);

            mockIncomeRepo.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateIncomeAsync_ReturnsFalse_WhenIncomeDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            var request = UpdateIncome();

            mockIncomeRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync((Income?)null);

            // Act
            var result = await incomeService.UpdateIncomeAsync(userId, incomeId, request);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteIncomeAsync_ReturnsTrue_WhenIncomeExists()
        {
            // Arrange
            var userId = 1;
            var incomeId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            var category = CreateCategory();
            var existingIncome = CreateIncome(category);

            mockIncomeRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync(existingIncome);

            mockUserRepo
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "testuser"
                });

            // Act
            var result = await incomeService.DeleteIncomeAsync(userId, incomeId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteIncomeAsync_ReturnsFalse_WhenIncomeDoesNotExist()
        {
            // Arrange
            var incomeId = 1;
            var userId = 1;

            var mockIncomeRepo = new Mock<IIncomeRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockMapper = new Mock<IMapper>();
            var incomeService = CreateIncomeService(mockIncomeRepo, mockUserRepo, mockMapper);

            mockIncomeRepo.Setup(x => x.GetIncomeByUserAsync(userId, incomeId))
                .ReturnsAsync((Income?)null);

            // Act
            var result = await incomeService.DeleteIncomeAsync(userId, incomeId);

            // Assert
            Assert.False(result);
        }

        // Helper Functions
        private static IncomeService CreateIncomeService(
            Mock<IIncomeRepository>? mockIncomeRepo = null,
            Mock<IUserRepository>? mockUserRepo = null,
            Mock<IMapper>? mockMapper = null)
        {
            return new IncomeService(
                mockIncomeRepo!.Object,
                mockUserRepo!.Object,
                mockMapper!.Object);
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
            Category category,
            int id = 1,
            int userId = 1,
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

        private static IncomeResDto CreateIncomeResponse(
            int id = 1,
            int userId = 1,
            string description = "Updated Income",
            decimal amount = 450m,
            string categoryName = "Salary",
            CategoryType categoryType = CategoryType.Income,
            DateTime? createdAt = null,
            DateTime? updatedAt = null)
        {
            return new IncomeResDto
            {
                Id = id,
                UserId = userId,
                Description = description,
                Amount = amount,
                CategoryName = categoryName,
                CategoryType = categoryType,
                CreatedAt = createdAt ?? new DateTime(2026, 1, 1),
                UpdatedAt = updatedAt
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
