using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Common;

namespace BudgetWise.BLL.Interfaces
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
