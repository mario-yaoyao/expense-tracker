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
        public async Task<ActionResult<ProfileResDto>> GetUserProfile()
        {
            try
            {
                var userId = GetUserId();
                var data = await userService.GetUserProfileAsync(userId);

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "User information not found."
                });

                return Ok(new ApiResDto<ProfileResDto>
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
                    ErrorMessage = $"An error occurred while retrieving user information. {ex.Message}"
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

                if (!result.Success)
                {
                    return BadRequest(new ApiResDto<bool?>
                    {
                        Success = false,
                        ErrorMessage = result.ErrorMessage
                    });
                }

                return Ok(new ApiResDto<bool?>
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while changing password. {ex.Message}"
                });
            }
        }
    }
}
