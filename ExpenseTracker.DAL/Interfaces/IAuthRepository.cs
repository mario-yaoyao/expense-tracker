using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserByResetToken(string resetToken);
        Task<bool> IsUsernameTakenAsync(string username);
        Task AddUserAsync(User user);
        Task UpdatePasswordAsync(User user);
        Task SaveChangesAsync();
    }
}
