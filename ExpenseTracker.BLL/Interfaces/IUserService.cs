using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
namespace ExpenseTracker.BLL.Interfaces
{
    public interface IUserService
    {
        Task<(List<UserResDto> data, int totalCount, bool hasNextPage)> GetUsersAsync(UserQueryReqDto request);
        Task<UserResDto?> GetUserByIdAsync(int userId);
        Task<UserResDto?> ToggleUserStatusAsync(string username, int userId);

    }
}
