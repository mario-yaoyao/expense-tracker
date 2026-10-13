using AutoMapper;
using BudgetWise.BLL.Interfaces;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Serilog;

namespace BudgetWise.BLL.Services
{
    public class CategoryService(ICategoryRepository categoryRepository, IUserRepository userRepository, IMapper mapper) : ICategoryService
    {
        public async Task<(List<CategoryResDto> data, bool hasNextPage)> GetCategoriesAsync(int userId, string role, CategoryQueryReqDto request)
        {
            List<Category> data;
            bool hasNextPage;

            if (role == "User")
            {
                (data, hasNextPage) = await categoryRepository.GetCategoriesByUserAsync(userId, request.Type, request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
            }
            else
            {
                (data, hasNextPage) = await categoryRepository.GetAllCategoriesAsync(request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
            }

            return (
                mapper.Map<List<CategoryResDto>>(data),
                hasNextPage
            );
        }

        public async Task<CategoryResDto?> GetCategoryByIdAsync(int userId, string role, int categoryId)
        {
            Category? category;

            if (role == "User")
            {
                category = await categoryRepository.GetCategoryByUserAsync(userId, categoryId);

                Console.WriteLine(category?.User?.Username);
            }
            else
            {
                category = await categoryRepository.GetCategoryByIdAsync(categoryId);
            }

            if (category == null) return null;

            return mapper.Map<CategoryResDto>(category);
        }

        public async Task<bool> CreateCategoryAsync(int userId, CreateCategoryReqDto category)
        {
            var exists = await categoryRepository.CategoryExistsAsync(
                userId,
                category.Name,
                category.Type);

            if (exists) return false;

            var newCategory = new Category
            {
                UserId = userId,
                Name = category.Name,
                Type = category.Type
            };

            await categoryRepository.AddCategoryAsync(newCategory);
            var user = await userRepository.GetUserByIdAsync(userId);

            Log.ForContext("IsAuditLog", true)
               .ForContext("UserId", userId)
               .ForContext("Username", user!.Username)
               .ForContext("Type", (int)TransactionType.Create)
               .ForContext("EntityName", "Category")
               .ForContext("Activity", $"Created category '{newCategory.Name}'.")
               .Information($"'{user.Username}' created category '{newCategory.Name}',");

            return true;
        }

        public async Task<bool> UpdateCategoryAsync(int userId, int categoryId, UpdateCategoryReqDto category)
        {
            var existingCategory = await categoryRepository.GetCategoryByUserAsync(userId, categoryId);

            if (existingCategory == null) return false;

            existingCategory.Name = string.IsNullOrWhiteSpace(category.Name)
                ? existingCategory.Name
                : category.Name;

            existingCategory.UpdatedAt = DateTime.UtcNow;

            await categoryRepository.SaveChangesAsync();
            var user = await userRepository.GetUserByIdAsync(userId);

            Log.ForContext("IsAuditLog", true)
               .ForContext("UserId", userId)
               .ForContext("Username", user!.Username)
               .ForContext("Type", (int)TransactionType.Update)
               .ForContext("EntityName", "Category")
               .ForContext("Activity", $"Updated category '{existingCategory.Name}'.")
               .Information($"'{user.Username}' updated category '{existingCategory.Name}',");

            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int userId, int categoryId)
        {
            var existingCategory = await categoryRepository.GetCategoryByUserAsync(userId, categoryId);

            if (existingCategory == null) return false;

            existingCategory.IsDeleted = true;

            await categoryRepository.SaveChangesAsync();
            var user = await userRepository.GetUserByIdAsync(userId);

            Log.ForContext("IsAuditLog", true)
               .ForContext("UserId", userId)
               .ForContext("Username", user!.Username)
               .ForContext("Type", (int)TransactionType.Delete)
               .ForContext("EntityName", "Category")
               .ForContext("Activity", $"Deleted category '{existingCategory.Name}'.")
               .Information($"'{user.Username}' deleted category '{existingCategory.Name}'.");

            return true;
        }
    }
}
