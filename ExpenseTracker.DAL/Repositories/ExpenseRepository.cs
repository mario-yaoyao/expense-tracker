using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.DAL.Repositories
{
    public class ExpenseRepository(AppDbContext context, ILogger<ExpenseRepository> logger) : IExpenseRepository
    {
        public async Task<(List<Expense> data, int totalCount, bool hasNextPage)> GetAllExpensesAsync(int page = 1, int limit = 20, string? search = null)
        {
            try
            {
                var query = context.Expenses
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(e => e.Description.Contains(search));
                }

                var totalCount = await query.CountAsync();

                var data = await query
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var hasNextPage = (page * limit) < totalCount;

                return (data, totalCount, hasNextPage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving all expenses: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<(List<Expense> data, decimal totalExpense, HighestRecordResDto? highestExpense, int totalCount, bool hasNextPage)> GetExpensesByUserAsync(int userId, int page = 1, int limit = 12, string? search = null)
        {
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                var query = context.Expenses
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .Where(e =>
                        e.UserId == userId &&
                        !e.IsDeleted &&
                        e.CreatedAt.Month == currentMonth &&
                        e.CreatedAt.Year == currentYear)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(e => e.Description.Contains(search));
                }

                var totalCount = await query.CountAsync();

                var totalExpense = await query.SumAsync(e => e.Amount);

                var highestExpense = await query
                    .OrderByDescending(e => e.Amount)
                    .Select(e => new HighestRecordResDto
                    {
                        Name = e.Category.Name,
                        Amount = e.Amount
                    })
                    .FirstOrDefaultAsync();

                var data = await query
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var hasNextPage = (page * limit) < totalCount;

                return (data, totalExpense, highestExpense, totalCount, hasNextPage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving expenses for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Expense?> GetExpenseByUserAsync(int userId, int expenseId)
        {
            try
            {
                return await context.Expenses
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .Where(e => !e.IsDeleted)
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == expenseId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving expense for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
        {
            try
            {
                return await context.Expenses
                    .Include(e => e.Category)
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
