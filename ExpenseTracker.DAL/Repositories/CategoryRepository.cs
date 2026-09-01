using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.DAL.Repositories
{
    public class CategoryRepository(AppDbContext context, ILogger<CategoryRepository> logger) : ICategoryRepository
    {
        public async Task<(List<Category> data, bool hasNextPage)> GetAllCategoriesAsync(int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            try
            {
                var query = context.Categories.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(c => c.Name.Contains(search));
                }

                if (startDate.HasValue)
                {
                    var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(e => e.CreatedAt >= startDateTime);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(e => e.CreatedAt <= endDateTime);
                }

                var totalCount = await query.CountAsync();

                var data = await query
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var hasNextPage = (page * limit) < totalCount;

                return (data, hasNextPage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving all categories: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<(List<Category> data, bool hasNextPage)> GetCategoriesByUserAsync(int userId, CategoryType? type = null, int page = 1, int limit = 12, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            try
            {
                var query = context.Categories
                    .Where(c => c.UserId == userId && !c.IsDeleted);

                if (type.HasValue)
                {
                    query = query.Where(c => c.Type == type.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(c => c.Name.Contains(search));
                }

                if (startDate.HasValue)
                {
                    var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(e => e.CreatedAt >= startDateTime);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(e => e.CreatedAt <= endDateTime);
                }

                var totalCount = await query.CountAsync();

                query = type == CategoryType.Expense || type == CategoryType.Income
                    ? query.OrderBy(c => c.Name)
                    : query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt);

                var data = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var hasNextPage = (page * limit) < totalCount;

                return (data, hasNextPage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving categories for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Category?> GetCategoryByUserAsync(int userId, int categoryId)
        {
            try
            {
                return await context.Categories
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == categoryId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving category for user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            try
            {
                return await context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving category by ID: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> CategoryExistsAsync(int userId, string name, CategoryType type)
        {
            return await context.Categories
                .AnyAsync(c =>
                    c.UserId == userId &&
                    c.Name == name &&
                    c.Type == type &&
                    !c.IsDeleted);
        }

        public async Task AddCategoryAsync(Category category)
        {
            try
            {
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while adding category: {Message}", ex.Message);
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
                logger.LogError(ex, "Database error while saving category changes: {Message}", ex.Message);
                throw;
            }
        }
    }
}
