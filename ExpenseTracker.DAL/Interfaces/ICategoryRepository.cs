using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface ICategoryRepository
    {
        Task<(List<Category> data, int totalCount, bool hasNextPage)> GetAllCategoriesAsync(int page = 1, int limit = 12, string? search = null);
        Task<(List<Category> data, int totalCount, bool hasNextPage)> GetCategoriesByUserAsync(int userId, CategoryType? type = null, int page = 1, int limit = 12, string? search = null);
        Task<Category?> GetCategoryByUserAsync(int userId, int categoryId);
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task<bool> CategoryExistsAsync(int userId, string name, CategoryType type);
        Task AddCategoryAsync(Category category);
        Task SaveChangesAsync();
    }
}
