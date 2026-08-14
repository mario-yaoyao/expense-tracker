using ExpenseTracker.Models.Dtos.Responses;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IUserService
    {
        Task<UserResDto?> GetUserProfileAsync(Guid userId);
    }
}
