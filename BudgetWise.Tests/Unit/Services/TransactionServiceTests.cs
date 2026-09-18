using AutoMapper;
using BudgetWise.BLL.Services;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Moq;

namespace BudgetWise.Tests.Unit.Services
{
    public class TransactionServiceTests
    {
        [Fact]
        public async Task GetCategoriesAsync_ReturnsUserCategories_WhenRoleIsUser()
        {
            // Arrange
            var secondTransactionId = 2;
            var userId = 1;
            var transactionType = TransactionType.Create;

            var mockTransactionRepo = new Mock<ITransactionRepository>();
            var mockMapper = new Mock<IMapper>();
            var transactionService = CreateTransactionService(mockTransactionRepo, mockMapper);

            var queryReq = CreateQueryRequest();

            var transactions = new List<Transaction>
            {
                CreateTransaction(),
                CreateTransaction(id: secondTransactionId, message: "Updated category", type: TransactionType.Update, activity: "Updated category"),
            };

            var mappedTransactions = new List<TransactionResDto>
            {
                CreateTransactionResponse(id: transactions[0].Id, userId: transactions[0].UserId, type: transactions[0].Type, activity: transactions[0].Activity),
                CreateTransactionResponse(id: transactions[1].Id, userId: transactions[1].UserId, type: transactions[1].Type, activity: transactions[1].Activity)
            };

            var expectedResponse = (
                Data: transactions,
                HasNextPage: false
            );

            mockTransactionRepo
                .Setup(x => x.GetTransactionsByUserAsync(userId, queryReq.Type, queryReq.Page, queryReq.Limit, queryReq.Search))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<TransactionResDto>>(It.IsAny<List<Transaction>>()))
                .Returns(mappedTransactions);

            // Act
            var (data, _) = await transactionService.GetTransactionsAsync(userId, "User", queryReq);

            // Assert
            Assert.Equal(mappedTransactions[0].Type, data[0].Type);
            Assert.Equal(TransactionType.Create, data[0].Type);

            mockTransactionRepo.Verify(
                x => x.GetTransactionsByUserAsync(userId, transactionType, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetTransactionsAsync_ReturnsAllCategories_WhenRoleIsSuperAdmin()
        {
            // Arrange
            var secondTransactionId = 2;
            var thirdTransactionId = 3;
            var firstUserId = 1;
            var secondUserId = 2;
            var transactionType = TransactionType.Create;

            var mockTransactionRepo = new Mock<ITransactionRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateTransactionService(mockTransactionRepo, mockMapper);

            var queryReq = CreateQueryRequest();

            var transactions = new List<Transaction>
            {
                CreateTransaction(),
                CreateTransaction(id: secondTransactionId, message: "Updated category", type: TransactionType.Update, activity: "Updated category"),
                CreateTransaction(id: thirdTransactionId, userId: secondUserId, message: "Deleted category", type: TransactionType.Update, activity: "Deleted category"),
            };

            var mappedTransactions = new List<TransactionResDto>
            {
                CreateTransactionResponse(id: transactions[0].Id, userId: transactions[0].UserId, type: transactions[0].Type, activity: transactions[0].Activity),
                CreateTransactionResponse(id: transactions[1].Id, userId: transactions[1].UserId, type: transactions[1].Type, activity: transactions[1].Activity),
                CreateTransactionResponse(id: transactions[2].Id, userId: transactions[2].UserId, type: transactions[2].Type, activity: transactions[2].Activity)
            };

            var expectedResponse = (
                Data: transactions,
                HasNextPage: false
            );

            mockTransactionRepo
                .Setup(x => x.GetAllTransactionsAsync(queryReq.Type, queryReq.Page, queryReq.Limit, queryReq.Search))
                .ReturnsAsync(expectedResponse);

            mockMapper
                .Setup(x => x.Map<List<TransactionResDto>>(It.IsAny<List<Transaction>>()))
                .Returns(mappedTransactions);

            // Act
            var (data, _) = await categoryService.GetTransactionsAsync(firstUserId, "SuperAdmin", queryReq);

            // Assert
            Assert.Equal(mappedTransactions[0].Username, data[0].Username);
            Assert.Equal(mappedTransactions[1].Type, data[1].Type);
            Assert.Equal(mappedTransactions[2].Message, data[1].Message);

            mockTransactionRepo.Verify(
                x => x.GetAllTransactionsAsync(transactionType, 1, 20, null),
                Times.Once);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ReturnsTransaction_WhenFound()
        {
            // Arrange
            var transactionId = 1;
            var userId = 1;

            var mockTransactionRepo = new Mock<ITransactionRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateTransactionService(mockTransactionRepo, mockMapper);

            var transaction = CreateTransaction();
            var mappedCategory = CreateTransactionResponse();

            mockMapper
                .Setup(x => x.Map<TransactionResDto>(It.IsAny<Transaction>()))
                .Returns(mappedCategory);

            mockTransactionRepo.Setup(x => x.GetTransactionAsync(userId, transactionId))
                .ReturnsAsync(transaction);

            // Act
            var result = await categoryService.GetTransactionByIdAsync(userId, "User", transactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.Id);
            Assert.Equal(transaction.Username, result.Username);
            Assert.Equal(transaction.Type, result.Type);

            mockTransactionRepo.Verify(
                x => x.GetTransactionAsync(userId, transactionId),
                Times.Once);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ReturnsNull_WhenTransactionDoesNotExist()
        {
            // Arrange
            var transactionId = 1;
            var userId = 1;

            var mockTransactionRepo = new Mock<ITransactionRepository>();
            var mockMapper = new Mock<IMapper>();
            var categoryService = CreateTransactionService(mockTransactionRepo, mockMapper);

            mockTransactionRepo.Setup(x => x.GetTransactionAsync(userId, transactionId))
                .ReturnsAsync((Transaction?)null);

            // Act
            var result = await categoryService.GetTransactionByIdAsync(userId, "User", transactionId);

            // Assert
            Assert.Null(result);

            mockTransactionRepo.Verify(
                x => x.GetTransactionAsync(userId, transactionId),
                Times.Once);
        }

        // Helper Functions
        private static TransactionService CreateTransactionService(
            Mock<ITransactionRepository>? mockTransactionRepo = null,
            Mock<IMapper>? mockMapper = null)
        {
            return new TransactionService(
                mockTransactionRepo!.Object,
                mockMapper!.Object);
        }

        private static Transaction CreateTransaction(
            int id = 1,
            string message = "Created category",
            string level = "Information",
            DateTime? timestamp = null,
            string? exception = null,
            int? userId = 1,
            string? username = "testuser",
            TransactionType? type = TransactionType.Create,
            string? entityName = "Category",
            string? activity = "Created category")
        {
            return new Transaction
            {
                Id = id,
                Message = message,
                Level = level,
                TimeStamp = timestamp ?? DateTime.UtcNow,
                Exception = exception,
                UserId = userId,
                Username = username,
                Type = type,
                EntityName = entityName,
                Activity = activity
            };
        }

        private static TransactionResDto CreateTransactionResponse(
            int id = 1,
            string message = "Created category",
            DateTime? timestamp = null,
            int? userId = 1,
            string? username = "testuser",
            TransactionType? type = TransactionType.Create,
            string? activity = "Created category")
        {
            return new TransactionResDto
            {
                Id = id,
                Message = message,
                TimeStamp = timestamp ?? DateTime.UtcNow,
                UserId = userId,
                Username = username,
                Type = type,
                Activity = activity
            };
        }

        private static TransactionQueryReqDto CreateQueryRequest(
            TransactionType type = TransactionType.Create,
            int page = 1,
            int limit = 20,
            string? search = null)
        {
            return new TransactionQueryReqDto
            {
                Type = type,
                Page = page,
                Limit = limit,
                Search = search
            };
        }
    }
}
