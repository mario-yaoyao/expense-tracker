using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
namespace BudgetWise.BLL.Interfaces
{
    public interface IUserService
    {
        Task<(List<UserResDto> data, int totalCount, bool hasNextPage)> GetUsersAsync(UserQueryReqDto request);
        Task<UserResDto?> GetUserByIdAsync(int userId);
        Task<bool> ToggleUserStatusAsync(string username, int userId);

    }
}
