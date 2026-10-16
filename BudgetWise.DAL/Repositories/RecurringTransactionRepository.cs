using BudgetWise.DAL.Data;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetWise.DAL.Repositories
{
    public class RecurringTransactionRepository(AppDbContext context, ILogger<CategoryRepository> logger) : IRecurringTransactionRepository
    {
        public async Task<List<RecurringTransaction>> GetActiveRecurringTransactionsAsync()
        {
            try
            {
                return await context.RecurringTransactions
                    .Include(r => r.User)
                    .Include(r => r.Category)
                    .Where(r => r.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Database error while retrieving active recurring transactions: {Message}",
                    ex.Message);

                throw;
            }
        }

        public async Task AddRecurringTransactionAsync(RecurringTransaction recurringTransaction)
        {
            try
            {
                await context.RecurringTransactions.AddAsync(recurringTransaction);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while adding recurring transaction: {Message}", ex.Message);
                throw;
            }
        }
    }
}
