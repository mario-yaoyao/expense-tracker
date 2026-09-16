using BudgetWise.DAL.Data;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetWise.DAL.Repositories
{
    public class UserRepository(AppDbContext context, ILogger<UserRepository> logger) : IUserRepository
    {
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<(List<User> data, int totalCount, bool hasNextPage)> GetUsersAsync(int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            try
            {
                var query = context.Users
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(i => i.Username.Contains(search) || i.FullName.Contains(search));
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
                logger.LogError(ex, "Database error while retrieving all users: {Message}", ex.Message);
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
                logger.LogError(ex, "Database error while saving user changes: {Message}", ex.Message);
                throw;
            }
        }
    }
}
