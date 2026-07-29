using expense_tracker.Dtos.Requests;
using expense_tracker.Dtos.Responses;

namespace expense_tracker.Services
{
    public interface IExpenseService
    {
        Task<List<ExpenseResDto>> GetExpensesAsync();
        Task<ExpenseResDto?> GetExpenseByIdAsync(Guid id);
        Task<ExpenseResDto?> CreateExpenseAsync(ExpenseReqDto expense);
        Task<ExpenseResDto?> UpdateExpenseAsync(Guid id, ExpenseReqDto expense);
        Task<bool> DeleteExpenseAsync(Guid id);
    }
}
