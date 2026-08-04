using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<ApiResDto<RegisterResDto>>> Register(RegisterReqDto request)
        {
            try
            {
                var result = await authService.RegisterAsync(request);

                if (!result.success)
                {
                    return BadRequest(new ApiResDto<RegisterResDto>
                    {
                        success = false,
                        message = result.message
                    });
                }

                return Ok(new ApiResDto<RegisterResDto>
                {
                    success = true,
                    message = result.message,
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = $"An error occurred while registering account: {ex.Message}"
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResDto<TokenResDto>>> Login(LoginUserReqDto request)
        {
            try
            {
                var result = await authService.LoginAsync(request);

                if (!result.success)
                {
                    return BadRequest(new ApiResDto<TokenResDto>
                    {
                        success = false,
                        message = result.message
                    });
                }

                return Ok(new ApiResDto<TokenResDto>
                {
                    success = true,
                    message = result.message,
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = $"An error occurred while logging in: {ex.Message}"
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResDto<TokenResDto>>> RefreshToken(RefreshTokenReqDto request)
        {
            try
            {
                var data = await authService.RefreshTokensAsync(request);
                if (data == null)
                {
                    return Unauthorized(new ApiResDto<TokenResDto>
                    {
                        success = false,
                        message = "Invalid refresh token."
                    });
                }

                return Ok(new ApiResDto<TokenResDto>
                {
                    success = true,
                    message = "Token refreshed successfully.",
                    data = data
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = "An error occurred while refreshing token."
                });
            }
        }
    }
}
