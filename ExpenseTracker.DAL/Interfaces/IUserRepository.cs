using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserProfileAsync(Guid userId);
    }
}
