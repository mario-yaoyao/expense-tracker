using ExpenseTracker.Models.Common;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileResDto?> GetUserProfileAsync(int userId);
        Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordReqDto request);
    }
}
