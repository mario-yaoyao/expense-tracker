using BudgetWise.Models.Models;

namespace BudgetWise.DAL.Interfaces
{
    public interface ITransactionRepository
    {
        Task<(List<Transaction> data, bool hasNextPage)> GetAllTransactionsAsync(TransactionType? type = null, int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null);
        Task<(List<Transaction> data, bool hasNextPage)> GetTransactionsByUserAsync(int userId, TransactionType? type = null, int page = 1, int limit = 12, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null);
        Task<Transaction?> GetTransactionAsync(int userId, int transactionId);
        Task<Transaction?> GetOwnTransactionAsync(int transactionId);
    }
}
