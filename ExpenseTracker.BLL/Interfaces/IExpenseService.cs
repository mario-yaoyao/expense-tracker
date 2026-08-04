using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IExpenseService
    {
        Task<List<ExpenseResDto>> GetExpensesAsync(Guid userId, string role);
        Task<ExpenseResDto?> GetExpenseByIdAsync(Guid userId, string role, Guid id);
        Task<ExpenseResDto?> CreateExpenseAsync(Guid userId, ExpenseReqDto expense);
        Task<ExpenseResDto?> UpdateExpenseAsync(Guid userId, string role, Guid id, ExpenseReqDto expense);
        Task<bool> DeleteExpenseAsync(Guid userId, string role, Guid id);
    }
}
