using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
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
        public async Task<ActionResult<ApiResDto<UserResDto>>> ToggleUserStatus(int userId)
        {
            try
            {
                var username = GetUsername();
                var data = await userService.ToggleUserStatusAsync(username, userId);

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
                    ErrorMessage = "An error occurred while deactivating a user account."
                });
            }
        }
    }
}
