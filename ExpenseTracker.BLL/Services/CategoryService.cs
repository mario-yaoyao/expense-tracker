using AutoMapper;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Services
{
    public class CategoryService(ICategoryRepository categoryRepository, IMapper mapper) : ICategoryService
    {
        public async Task<(List<CategoryResDto> data, int totalCount, bool hasNextPage)> GetCategoriesAsync(int userId, string role, CategoryType? type = null, int page = 1, int limit = 12, string? search = null)
        {
            List<Category> data;
            int totalCount;
            bool hasNextPage;

            if (role == "User")
            {
                (data, totalCount, hasNextPage) = await categoryRepository.GetCategoriesByUserAsync(userId, type, page, limit, search); // get data, totalCount, hasNextPage
            }
            else
            {
                (data, totalCount, hasNextPage) = await categoryRepository.GetAllCategoriesAsync(page, limit, search);
            }

            return (
                mapper.Map<List<CategoryResDto>>(data),
                totalCount,
                hasNextPage
            );
        }

        public async Task<CategoryResDto?> GetCategoryByIdAsync(int userId, string role, int categoryId)
        {
            Category? category;

            if (role == "User")
            {
                category = await categoryRepository.GetCategoryByUserAsync(userId, categoryId);
            }
            else
            {
                category = await categoryRepository.GetCategoryByIdAsync(categoryId);
            }

            if (category == null) return null;

            return mapper.Map<CategoryResDto>(category);
        }

        public async Task<CategoryResDto?> CreateCategoryAsync(int userId, CreateCategoryReqDto category)
        {
            var exists = await categoryRepository.CategoryExistsAsync(
                userId,
                category.Name,
                category.Type);

            if (exists) return null;

            var newCategory = new Category
            {
                UserId = userId,
                Name = category.Name,
                Type = category.Type
            };

            await categoryRepository.AddCategoryAsync(newCategory);

            return mapper.Map<CategoryResDto>(newCategory);
        }

        public async Task<CategoryResDto?> UpdateCategoryAsync(int userId, int categoryId, UpdateCategoryReqDto category)
        {
            var existingCategory = await categoryRepository.GetCategoryByUserAsync(userId, categoryId);

            if (existingCategory == null) return null;

            existingCategory.Name = string.IsNullOrWhiteSpace(category.Name)
                ? existingCategory.Name
                : category.Name;

            existingCategory.Type = category.Type ?? existingCategory.Type;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            await categoryRepository.SaveChangesAsync();

            return mapper.Map<CategoryResDto>(existingCategory);
        }

        //NOTE: soft delete the category by setting IsDeleted to true
        public async Task<bool> DeleteCategoryAsync(int userId, int categoryId)
        {
            var existingCategory = await categoryRepository.GetCategoryByUserAsync(userId, categoryId);

            if (existingCategory == null) return false;

            existingCategory.IsDeleted = true;

            await categoryRepository.SaveChangesAsync();

            return true;
        }
    }
}
