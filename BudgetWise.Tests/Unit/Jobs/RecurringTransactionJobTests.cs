using BudgetWise.BLL.Interfaces;
using BudgetWise.BLL.Jobs;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Models;
using Moq;

namespace BudgetWise.Tests.Unit.Jobs
{
    public class RecurringTransactionJobTests
    {
        [Fact]
        public async Task ProcessRecurringTransactionsAsync_CreatesIncome_WhenTransactionIsIncome()
        {
            // Arrange
            var transactionId = 1;
            var userId = 1;

            var mockIncomeService = new Mock<IIncomeService>();
            var mockExpenseService = new Mock<IExpenseService>();
            var mockRecurringTransactionRepo = new Mock<IRecurringTransactionRepository>();

            var job = CreateRecurringTransactionJob(
                mockIncomeService,
                mockExpenseService,
                mockRecurringTransactionRepo);

            var recurringTransactions = new List<RecurringTransaction>
            {
                CreateRecurringTransaction(
                    id: transactionId,
                    userId: userId,
                    type: CategoryType.Income)
            };

            mockRecurringTransactionRepo
                .Setup(x => x.GetActiveRecurringTransactionsAsync())
                .ReturnsAsync(recurringTransactions);

            // Act
            await job.ProcessRecurringTransactionsAsync();

            // Assert
            mockIncomeService.Verify(
                x => x.CreateRecurringIncomeAsync(
                    userId,
                    recurringTransactions[0].Description,
                    recurringTransactions[0].Amount,
                    recurringTransactions[0].CategoryId,
                    transactionId),
                Times.Once);

            mockExpenseService.Verify(
                x => x.CreateRecurringExpenseAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessRecurringTransactionsAsync_CreatesExpense_WhenTransactionIsExpense()
        {
            // Arrange
            var transactionId = 1;
            var userId = 1;

            var mockIncomeService = new Mock<IIncomeService>();
            var mockExpenseService = new Mock<IExpenseService>();
            var mockRecurringTransactionRepo = new Mock<IRecurringTransactionRepository>();

            var job = CreateRecurringTransactionJob(
                mockIncomeService,
                mockExpenseService,
                mockRecurringTransactionRepo);

            var recurringTransactions = new List<RecurringTransaction>
            {
                CreateRecurringTransaction(
                    id: transactionId,
                    userId: userId,
                    type: CategoryType.Expense)
            };

            mockRecurringTransactionRepo
                .Setup(x => x.GetActiveRecurringTransactionsAsync())
                .ReturnsAsync(recurringTransactions);

            // Act
            await job.ProcessRecurringTransactionsAsync();

            // Assert
            mockExpenseService.Verify(
                x => x.CreateRecurringExpenseAsync(
                    userId,
                    recurringTransactions[0].Description,
                    recurringTransactions[0].Amount,
                    recurringTransactions[0].CategoryId,
                    transactionId),
                Times.Once);

            mockIncomeService.Verify(
                x => x.CreateRecurringIncomeAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessRecurringTransactionsAsync_DoesNothing_WhenNoActiveTransactionsExist()
        {
            // Arrange
            var mockIncomeService = new Mock<IIncomeService>();
            var mockExpenseService = new Mock<IExpenseService>();
            var mockRecurringTransactionRepo = new Mock<IRecurringTransactionRepository>();

            var job = CreateRecurringTransactionJob(
                mockIncomeService,
                mockExpenseService,
                mockRecurringTransactionRepo);

            mockRecurringTransactionRepo
                .Setup(x => x.GetActiveRecurringTransactionsAsync())
                .ReturnsAsync(new List<RecurringTransaction>());

            // Act
            await job.ProcessRecurringTransactionsAsync();

            // Assert
            mockIncomeService.Verify(
                x => x.CreateRecurringIncomeAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);

            mockExpenseService.Verify(
                x => x.CreateRecurringExpenseAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        // Helper Functions
        private static RecurringTransactionJob CreateRecurringTransactionJob(
            Mock<IIncomeService>? mockIncomeService = null,
            Mock<IExpenseService>? mockExpenseService = null,
            Mock<IRecurringTransactionRepository>? mockRecurringTransactionRepo = null)
        {
            return new RecurringTransactionJob(
                mockIncomeService!.Object,
                mockExpenseService!.Object,
                mockRecurringTransactionRepo!.Object);
        }

        private static RecurringTransaction CreateRecurringTransaction(
            int id = 1,
            int userId = 1,
            string description = "Salary",
            decimal amount = 1000,
            int categoryId = 1,
            CategoryType type = CategoryType.Income)
        {
            return new RecurringTransaction
            {
                Id = id,
                UserId = userId,
                Description = description,
                Amount = amount,
                CategoryId = categoryId,
                Type = type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}