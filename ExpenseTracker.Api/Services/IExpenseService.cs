using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;

namespace ExpenseTracker.Services
{
    public interface IExpenseService
    {
        Task<List<ExpenseResDto>> GetExpensesAsync();
        Task<ExpenseResDto?> GetExpenseByIdAsync(Guid id);
        Task<ExpenseResDto?> CreateExpenseAsync(Guid userId, ExpenseReqDto expense);
        Task<ExpenseResDto?> UpdateExpenseAsync(Guid id, ExpenseReqDto expense);
        Task<bool> DeleteExpenseAsync(Guid id);
    }
}
