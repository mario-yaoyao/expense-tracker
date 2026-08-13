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

                if (!result.Success)
                {
                    return BadRequest(new ApiResDto<RegisterResDto>
                    {
                        Success = false,
                        ErrorMessage = result.ErrorMessage
                    });
                }

                return Ok(new ApiResDto<RegisterResDto>
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
                    ErrorMessage = $"An error occurred while registering account: {ex.Message}"
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResDto<TokenResDto>>> Login(LoginUserReqDto request)
        {
            try
            {
                var result = await authService.LoginAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new ApiResDto<TokenResDto>
                    {
                        Success = false,
                        ErrorMessage = result.ErrorMessage
                    });
                }

                return Ok(new ApiResDto<TokenResDto>
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
                    ErrorMessage = $"An error occurred while logging in: {ex.Message}"
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
                        Success = false,
                        ErrorMessage = "Invalid refresh token."
                    });
                }

                return Ok(new ApiResDto<TokenResDto>
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
                    ErrorMessage = "An error occurred while refreshing token."
                });
            }
        }
    }
}
