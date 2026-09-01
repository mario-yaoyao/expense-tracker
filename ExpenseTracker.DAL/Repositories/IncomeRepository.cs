
using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.DAL.Repositories
{
    public class IncomeRepository(AppDbContext context, ILogger<IncomeRepository> logger) : IIncomeRepository
    {
        public async Task<(List<Income> data, int totalCount, bool hasNextPage)> GetAllIncomesAsync(int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            try
            {
                var query = context.Incomes
                    .Include(i => i.Category)
                    .Include(i => i.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(i => i.Description.Contains(search));
                }

                if (startDate.HasValue)
                {
                    var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(i => i.CreatedAt >= startDateTime);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(i => i.CreatedAt <= endDateTime);
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
                logger.LogError(ex, "Database error while retrieving all incomes: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<(List<Income> data, decimal totalIncome, HighestAmountResDto? highestIncome, int totalCount, bool hasNextPage)> GetIncomesByUserAsync(int userId, int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                var query = context.Incomes
                    .Include(i => i.Category)
                    .Include(i => i.User)
                    .Where(i =>
                        i.UserId == userId &&
                        !i.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(i => i.Description.Contains(search));
                }

                if (startDate.HasValue)
                {
                    var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(i => i.CreatedAt >= startDateTime);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(i => i.CreatedAt <= endDateTime);
                }

                var totalCount = await query.CountAsync();

                var totalIncome = await query.SumAsync(i => i.Amount);

                var highestIncome = await query
                    .OrderByDescending(i => i.Amount)
                    .Select(i => new HighestAmountResDto
                    {
                        Name = i.Category.Name,
                        Amount = i.Amount
                    })
                    .FirstOrDefaultAsync();

                var data = await query
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var hasNextPage = (page * limit) < totalCount;

                return (data, totalIncome, highestIncome, totalCount, hasNextPage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving incomes for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Income?> GetIncomeByUserAsync(int userId, int incomeId)
        {
            try
            {
                return await context.Incomes
                    .Include(i => i.Category)
                    .Include(i => i.User)
                    .Where(i => !i.IsDeleted)
                    .FirstOrDefaultAsync(i => i.UserId == userId && i.Id == incomeId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving income for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Income?> GetIncomeByIdAsync(int incomeId)
        {
            try
            {
                return await context.Incomes
                    .Include(i => i.Category)
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == incomeId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving income by ID: {Message}", ex.Message);
                throw;
            }
        }

        public async Task AddIncomeAsync(Income income)
        {
            try
            {
                await context.Incomes.AddAsync(income);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while adding income: {Message}", ex.Message);
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
                logger.LogError(ex, "Database error while saving income changes: {Message}", ex.Message);
                throw;
            }
        }
    }
}
