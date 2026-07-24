using expense_tracker.Dtos;

namespace expense_tracker.Services
{
    public interface IExpenseService
    {
        Task<List<ExpenseDto>> GetExpensesAsync();
        Task<ExpenseDto?> GetExpenseByIdAsync(int id);
        Task<ExpenseDto?> CreateExpenseAsync(ExpenseDto expense);
        Task<ExpenseDto?> UpdateExpenseAsync(int id, ExpenseDto expense);
        Task<List<ExpenseDto?>> DeleteExpenseAsync(int id);
    }
}
