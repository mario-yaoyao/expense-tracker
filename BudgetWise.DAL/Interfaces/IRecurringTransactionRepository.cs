using BudgetWise.Models.Models;

namespace BudgetWise.DAL.Interfaces
{
    public interface IRecurringTransactionRepository
    {
        Task<List<RecurringTransaction>> GetActiveRecurringTransactionsAsync();
        Task AddRecurringTransactionAsync(RecurringTransaction recurringTransaction);
    }
}
