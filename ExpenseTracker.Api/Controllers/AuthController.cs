using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
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

                return result.Success
                    ? Ok(new ApiResDto<RegisterResDto>
                    {
                        Success = true,
                        Data = result.Data
                    })
                    : BadRequest(new ApiResDto<RegisterResDto>
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
                    ErrorMessage = "An error occurred while registering account."
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResDto<TokenResDto>>> Login(LoginUserReqDto request)
        {
            try
            {
                var result = await authService.LoginAsync(request);

                return result.Success
                    ? Ok(new ApiResDto<TokenResDto>
                    {
                        Success = true,
                        Data = result.Data
                    })
                    : BadRequest(new ApiResDto<TokenResDto>
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
                    ErrorMessage = "An error occurred while logging in."
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResDto<TokenResDto>>> RefreshToken(RefreshTokenReqDto request)
        {
            try
            {
                var data = await authService.RefreshTokensAsync(request);

                return data == null
                    ? Unauthorized(new ApiResDto<TokenResDto>
                    {
                        Success = false,
                        ErrorMessage = "Invalid refresh token."
                    })
                    : Ok(new ApiResDto<TokenResDto>
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

        [HttpPatch("forgot-password")]
        public async Task<ActionResult<ApiResDto<bool>>> ForgotPassword(ForgotPasswordReqDto request)
        {
            try
            {
                var data = await authService.ForgotPasswordAsync(request);

                return !data
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Account not found."
                    })
                    : Ok(new ApiResDto<object>
                    {
                        Success = true,
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while resetting password."
                });
            }
        }

        [HttpPatch("reset-password")]
        public async Task<ActionResult<ApiResDto<bool>>> ChangePassword([FromBody] ResetPasswordReqDto request)
        {
            try
            {
                var result = await authService.ResetPasswordAsync(request);

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
                    ErrorMessage = "An error occurred while resetting password."
                });
            }
        }
    }
}
