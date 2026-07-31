using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Services;
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
                var (data, error) = await authService.RegisterAsync(request);
                if (error != null)
                {
                    return BadRequest(new ApiResDto<RegisterResDto>
                    {
                        success = false,
                        message = error switch
                        {
                            "password_mismatch" => "Passwords do not match.",
                            "duplicate_name" => "Username is already taken.",
                            _ => "Registration failed."
                        }
                    });
                }

                return Ok(new ApiResDto<RegisterResDto>
                {
                    success = true,
                    message = "Registration completed successfully. You can now log in to your account.",
                    data = new RegisterResDto
                    {
                        UserId = data!.Id,
                        FullName = data.FullName,
                        Username = data.Username,
                        ContactNumber = data.ContactNumber,
                        Role = data.Role,
                        IsActive = data.IsActive,   
                        CreatedAt = data.CreatedAt
                    }
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
                var (token, error) = await authService.LoginAsync(request);
                if (error != null)
                    return BadRequest(new ApiResDto<TokenResDto>
                    {
                        success = false,
                        message = error switch
                        {
                            "user_not_found" => "No account found with that username.",
                            "invalid_password" => "Incorrect password.",
                            "account_inactive" => "Your account has been deactivated. Please contact support for assistance.",
                            _ => "Login failed."
                        }
                    });

                return Ok(new ApiResDto<TokenResDto>
                {
                    success = true,
                    message = "Login successful.",
                    data = token
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
