using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface ICategoryService
    {
        Task<(List<CategoryResDto> data, bool hasNextPage)> GetCategoriesAsync(int userId, string role, CategoryQueryReqDto request);
        Task<CategoryResDto?> GetCategoryByIdAsync(int userId, string role, int categoryId);
        Task<bool> CreateCategoryAsync(int userId, CreateCategoryReqDto category);
        Task<bool> UpdateCategoryAsync(int userId, int categoryId, UpdateCategoryReqDto category);
        Task<bool> DeleteCategoryAsync(int userId, int categoryId);
    }
}
