using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResDto>> GetCategoriesAsync(int userId, string role, CategoryType? type = null);
        Task<CategoryResDto?> GetCategoryByIdAsync(int userId, string role, int categoryId);
        Task<CategoryResDto?> CreateCategoryAsync(int userId, CreateCategoryReqDto category);
        Task<CategoryResDto?> UpdateCategoryAsync(int userId, int categoryId, UpdateCategoryReqDto category);
        Task<bool> DeleteCategoryAsync(int userId, int categoryId);
    }
}
