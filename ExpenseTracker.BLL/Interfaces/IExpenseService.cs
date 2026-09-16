using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IExpenseService
    {
        Task<(List<ExpenseResDto> data, decimal totalExpense, HighestAmountResDto? highestExpense, int totalCount, bool hasNextPage)> GetExpensesAsync(int userId, string role, ExpenseQueryReqDto request);
        Task<ExpenseResDto?> GetExpenseByIdAsync(int userId, string role, int id);
        Task<bool> CreateExpenseAsync(int userId, CreateExpenseReqDto expense);
        Task<bool> UpdateExpenseAsync(int userId, int id, UpdateExpenseReqDto expense);
        Task<bool> DeleteExpenseAsync(int userId, int id);
    }
}
