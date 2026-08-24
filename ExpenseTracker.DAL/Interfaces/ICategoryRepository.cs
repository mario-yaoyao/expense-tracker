using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Category>> GetCategoriesByUserAsync(int userId, CategoryType? type = null);
        Task<Category?> GetCategoryByUserAsync(int userId, int categoryId);
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task<bool> CategoryExistsAsync(int userId, string name, CategoryType type);
        Task AddCategoryAsync(Category category);
        Task SaveChangesAsync();
    }
}
