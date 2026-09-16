using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;

namespace BudgetWise.BLL.Interfaces
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
