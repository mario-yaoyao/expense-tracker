using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
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
        public async Task<ActionResult<ApiResDto<CategoriesResDto>>> GetCategories([FromQuery] CategoryQueryReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var (data, hasNextPage) = await categoryService.GetCategoriesAsync(userId, role, request);

                return Ok(new ApiResDto<CategoriesResDto>
                {
                    Success = true,
                    Data = new CategoriesResDto
                    {
                        Items = data,
                        Pagination = new PaginatedResDto
                        {
                            Page = request.Page,
                            Limit = request.Limit,
                            HasNextPage = hasNextPage
                        }
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving categories."
                });
            }
        }

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
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving the category."
                });
            }
        }

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
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating the category."
                });
            }
        }

        [HttpPatch("{categoryId}")]
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
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the category."
                });
            }
        }

        [HttpDelete("{categoryId}")]
        public async Task<ActionResult<CategoryResDto?>> DeleteCategory(int categoryId)
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
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while deleting the category."
                });
            }
        }
    }
}
