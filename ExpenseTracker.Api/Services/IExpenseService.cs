using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;

namespace ExpenseTracker.Services
{
    public interface IExpenseService
    {
        Task<List<ExpenseResDto>> GetExpensesAsync(Guid userId, string role);
        Task<ExpenseResDto?> GetExpenseByIdAsync(Guid userId, string role, Guid id);
        Task<ExpenseResDto?> CreateExpenseAsync(Guid userId, string role, ExpenseReqDto expense);
        Task<ExpenseResDto?> UpdateExpenseAsync(Guid userId, string role, Guid id, ExpenseReqDto expense);
        Task<bool> DeleteExpenseAsync(Guid userId, Guid id);
    }
}
