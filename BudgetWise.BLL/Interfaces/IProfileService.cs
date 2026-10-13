using BudgetWise.Models.Common;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;

namespace BudgetWise.BLL.Interfaces
{
    public interface IProfileService
    {
        Task<UserResDto?> GetUserProfileAsync(int userId);
        Task<ServiceResult<object>> ChangePasswordAsync(int userId, ChangePasswordReqDto request);
    }
}
