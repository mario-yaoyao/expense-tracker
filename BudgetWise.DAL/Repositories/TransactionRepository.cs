using BudgetWise.DAL.Data;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetWise.DAL.Repositories
{
    public class TransactionRepository(AppDbContext context, ILogger<TransactionRepository> logger) : ITransactionRepository
    {
        public async Task<(List<Transaction> data, bool hasNextPage)> GetAllTransactionsAsync(TransactionType? type = null, int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            try
            {
                var query = context.Transactions.AsQueryable();

                if (type.HasValue)
                {
                    query = query.Where(c => c.Type == type.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(t => t.Message.Contains(search));
                }

                if (startDate.HasValue)
                {
                    var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(t => t.TimeStamp >= startDateTime);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(t => t.TimeStamp <= endDateTime);
                }

                var totalCount = await query.CountAsync();

                query = type == TransactionType.Create || type == TransactionType.Update || type == TransactionType.Delete || type == TransactionType.Info
                    ? query.OrderBy(c => c.Message)
                    : query.OrderByDescending(c => c.TimeStamp);

                var data = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var hasNextPage = (page * limit) < totalCount;

                return (data, hasNextPage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving all transactions: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<(List<Transaction> data, bool hasNextPage)> GetTransactionsByUserAsync(int userId, TransactionType? type = null, int page = 1, int limit = 12, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            try
            {
                var query = context.Transactions
                    .Where(t => t.UserId == userId);

                if (type.HasValue)
                {
                    query = query.Where(c => c.Type == type.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(t => t.Activity!.Contains(search));
                }

                if (startDate.HasValue)
                {
                    var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(e => e.TimeStamp >= startDateTime);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(e => e.TimeStamp <= endDateTime);
                }

                var totalCount = await query.CountAsync();

                query = type == TransactionType.Create || type == TransactionType.Update || type == TransactionType.Delete || type == TransactionType.Info
                    ? query.OrderBy(c => c.Activity)
                    : query.OrderByDescending(c => c.TimeStamp);

                var data = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var hasNextPage = (page * limit) < totalCount;

                return (data, hasNextPage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving transactions for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Transaction?> GetTransactionAsync(int userId, int transactionId)
        {
            try
            {
                return await context.Transactions
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == transactionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving transaction for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Transaction?> GetOwnTransactionAsync(int transactionId)
        {
            try
            {
                return await context.Transactions
                    .FirstOrDefaultAsync(c => c.Id == transactionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving own transaction: {Message}", ex.Message);
                throw;
            }
        }
    }
}
