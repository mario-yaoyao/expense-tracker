using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IExpenseRepository
    {
        Task<(List<Expense> data, int totalCount, bool hasNextPage)> GetAllExpensesAsync(int page = 1, int limit = 20, string? search = null);
        Task<(List<Expense> data, decimal totalExpense, HighestRecordResDto? highestExpense, int totalCount, bool hasNextPage)> GetExpensesByUserAsync(int userId, int page = 1, int limit = 20, string? search = null);
        Task<Expense?> GetExpenseByUserAsync(int userId, int expenseId);
        Task<Expense?> GetExpenseByIdAsync(int expenseId);
        Task AddExpenseAsync(Expense expense);
        Task SaveChangesAsync();
    }
}
