using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Common;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<object>> RegisterAsync(EncryptedReqDto request);
        Task<ServiceResult<TokenResDto>> LoginAsync(EncryptedReqDto request);
        Task<TokenResDto?> RefreshTokensAsync(EncryptedReqDto request);
        Task<bool> ForgotPasswordAsync(EncryptedReqDto request);
        Task<ServiceResult<object>> ResetPasswordAsync(EncryptedReqDto request);
    }
}
