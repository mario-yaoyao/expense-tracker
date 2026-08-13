using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.DAL.Repositories
{
    public class ExpenseRepository(AppDbContext context, ILogger<ExpenseRepository> logger) : IExpenseRepository
    {
        public async Task<List<Expense>> GetAllExpensesAsync()
        {
            try
            {
                return await context.Expenses
                .Include(e => e.User)
                .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving all expenses: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<Expense>> GetExpensesByUserAsync(Guid userId)
        {
            try
            {
                return await context.Expenses
                    .Where(e => e.UserId == userId && !e.IsDeleted)
                    .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving expenses for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Expense?> GetExpenseByUserAsync(Guid userId, Guid expenseId)
        {
            try
            {
                return await context.Expenses
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == expenseId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving expense for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Expense?> GetExpenseByIdAsync(Guid expenseId)
        {
            try
            {
                return await context.Expenses
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.Id == expenseId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving expense by ID: {Message}", ex.Message);
                throw;
            }
        }

        public async Task AddExpenseAsync(Expense expense)
        {
            try
            {
                await context.Expenses.AddAsync(expense);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while adding expense: {Message}", ex.Message);
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while saving expense changes: {Message}", ex.Message);
                throw;
            }
        }
    }
}
