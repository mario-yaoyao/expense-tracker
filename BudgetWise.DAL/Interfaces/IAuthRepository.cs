using BudgetWise.Models.Models;

namespace BudgetWise.DAL.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserByResetToken(string resetToken);
        Task<bool> IsUsernameTakenAsync(string username);
        Task<bool> IsEmailTakenAsync(string email);
        Task<bool> IsContactNumberTakenAsync(string contactNumber);
        Task AddUserAsync(User user);
        Task UpdatePasswordAsync(User user);
        Task SaveChangesAsync();
    }
}
