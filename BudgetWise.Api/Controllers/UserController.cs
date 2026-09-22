using BudgetWise.BLL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetWise.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/users")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private string GetUsername() => User.FindFirstValue(ClaimTypes.Name)!;

        [HttpGet]
        public async Task<ActionResult<ApiResDto<UsersResDto>>> GetUsers([FromQuery] UserQueryReqDto request)
        {
            try
            {
                var (data, totalCount, hasNextPage) = await userService.GetUsersAsync(request);

                return Ok(new ApiResDto<UsersResDto>
                {
                    Success = true,
                    Data = new UsersResDto
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
                    ErrorMessage = "An error occurred while retrieving users."
                });
            }
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResDto<UserResDto>>> GetUserById(int userId)
        {
            try
            {
                var data = await userService.GetUserByIdAsync(userId);

                return data == null
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "User not found."
                    })
                    : Ok(new ApiResDto<UserResDto>
                    {
                        Success = true,
                        Data = data
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving user record."
                });
            }
        }

        [HttpPatch("{userId}")]
        public async Task<ActionResult<ApiResDto<object>>> ToggleUserStatus(int userId)
        {
            try
            {
                var username = GetUsername();
                var success = await userService.ToggleUserStatusAsync(username, userId);

                return !success
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "User not found."
                    })
                    : Ok(new ApiResDto<object>
                    {
                        Success = true
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while deactivating a user account."
                });
            }
        }
    }
}
