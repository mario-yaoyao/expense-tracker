using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Common;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<RegisterResDto>> RegisterAsync(RegisterReqDto request);
        Task<ServiceResult<TokenResDto>> LoginAsync(LoginUserReqDto request);
        Task<TokenResDto?> RefreshTokensAsync(RefreshTokenReqDto request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordReqDto request);
        Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordReqDto request);
    }
}
