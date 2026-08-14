using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<UserResDto>> GetUserProfile()
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

                return Ok(new ApiResDto<UserResDto>
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
    }
}
