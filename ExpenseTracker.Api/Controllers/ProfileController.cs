using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController(IProfileService userService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<UserResDto>> GetUserProfile()
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
        public async Task<ActionResult<bool?>> ChangePassword([FromBody] ChangePasswordReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var result = await userService.ChangePasswordAsync(userId, request);

                return result.Success
                    ? Ok(new ApiResDto<bool?>
                    {
                        Success = true,
                        Data = result.Data
                        
                    })
                    : BadRequest(new ApiResDto<bool?>
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
