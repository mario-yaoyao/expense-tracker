using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int userId);
        Task<bool> IsUsernameTakenAsync(string username);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
    }
}
