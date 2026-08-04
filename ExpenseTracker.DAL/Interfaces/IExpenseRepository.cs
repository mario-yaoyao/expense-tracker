using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetExpensesByUserAsync(Guid userId);
        Task<List<Expense>> GetAllExpensesAsync();
        Task<Expense?> GetExpenseByUserAsync(Guid userId, Guid expenseId);
        Task<Expense?> GetExpenseByIdAsync(Guid expenseId);
        Task AddExpenseAsync(Expense expense);
        Task SaveChangesAsync();
    }
}
