using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetExpensesByUserAsync(int userId);
        Task<List<Expense>> GetAllExpensesAsync();
        Task<Expense?> GetExpenseByUserAsync(int userId, int expenseId);
        Task<Expense?> GetExpenseByIdAsync(int expenseId);
        Task AddExpenseAsync(Expense expense);
        Task SaveChangesAsync();
    }
}
