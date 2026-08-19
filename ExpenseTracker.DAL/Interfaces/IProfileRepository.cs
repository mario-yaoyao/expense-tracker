using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IProfileRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdatePasswordAsync(User request);
    }
}
