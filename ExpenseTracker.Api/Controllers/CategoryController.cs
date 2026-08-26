using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/categories")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet]
        public async Task<ActionResult<ApiResDto<List<CategoryResDto>>>> GetCategories([FromQuery] CategoryType? type = null, [FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] string? search = null)
        {
            try
            {
                if (page < 1 || limit < 1)
                {
                    return BadRequest(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Page and limit must be greater than 0."
                    });
                }

                var userId = GetUserId();
                var role = GetRole();
                var result = await categoryService.GetCategoriesAsync(userId, role, type, page, limit, search);

                if (result.totalCount == 0) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "No categories found."
                });

                return Ok(new ApiResDto<List<CategoryResDto>>
                {
                    Success = true,
                    Data = result.data,
                    TotalCount = result.totalCount,
                    Page = page,
                    Limit = limit,
                    HasNextPage = result.hasNextPage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while retrieving categories. {ex.Message}"
                });
            }
        }

         //DONE: add endpoint for fetching a specific category details
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ApiResDto<CategoryResDto>>> GetCategoryById(int categoryId)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await categoryService.GetCategoryByIdAsync(userId, role, categoryId);

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "Category not found."
                });

                return Ok(new ApiResDto<CategoryResDto>
                {
                    Success = true,
                    Data = data,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while retrieving the category. {ex.Message}"
                });
            }
        }

        // DONE: add endpoint for adding a category
        [HttpPost]
        public async Task<ActionResult<ApiResDto<CategoryResDto>>> CreateCategory([FromBody] CreateCategoryReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var data = await categoryService.CreateCategoryAsync(userId, request);

                if (data == null)
                {
                    return BadRequest(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Category already exists"
                    });
                }

                return Ok(new ApiResDto<CategoryResDto>
                {
                    Success = true,
                    Data = data,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while creating the category. {ex.Message}"
                });
            }
        }

        // DONE: add endpoint for updating a category
        [HttpPut("{categoryId}")]
        public async Task<ActionResult<CategoryResDto>> UpdateCategory(int categoryId, [FromBody] UpdateCategoryReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var data = await categoryService.UpdateCategoryAsync(userId, categoryId, request);

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "Category not found."
                });

                return Ok(new ApiResDto<CategoryResDto>
                {
                    Success = true,
                    Data = data,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while updating the category. {ex.Message}"
                });
            }
        }

        // DONE: add endpoint for deleting (soft delete) a category
        [HttpDelete("{categoryId}")]
        public async Task<ActionResult<CategoryResDto?>> DeleteExpense(int categoryId)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await categoryService.DeleteCategoryAsync(userId, categoryId);

                if (!data) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "Category not found."
                });

                return Ok(new ApiResDto<CategoryResDto>
                {
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while deleting the category. {ex.Message}"
                });
            }
        }
    }
}
