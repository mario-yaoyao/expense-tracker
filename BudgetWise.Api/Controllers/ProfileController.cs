using BudgetWise.BLL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetWise.Controllers
{
    [Authorize]
    [Route("api/profile")]
    [ApiController]
    public class ProfileController(IProfileService userService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<ApiResDto<UserResDto>>> GetUserProfile()
        {
            try
            {
                var userId = GetUserId();
                var data = await userService.GetUserProfileAsync(userId);

                return data == null
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "User information not found."
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
                    ErrorMessage = "An error occurred while retrieving user information."
                });
            }
        }

        [HttpPatch("change-password")]
        public async Task<ActionResult<ApiResDto<object>>> ChangePassword([FromBody] ChangePasswordReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var result = await userService.ChangePasswordAsync(userId, request);

                return result.Success
                    ? Ok(new ApiResDto<object>
                    {
                        Success = true
                        
                    })
                    : BadRequest(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = result.ErrorMessage
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while changing password."
                });
            }
        }
    }
}
