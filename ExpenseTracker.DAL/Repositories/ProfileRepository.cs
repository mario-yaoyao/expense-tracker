using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.DAL.Repositories
{
    public class ProfileRepository(AppDbContext context, ILogger<ProfileRepository> logger) : IProfileRepository
    {
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task UpdatePasswordAsync(User user)
        {
            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while saving updating password: {Message}", ex.Message);
                throw;
            }
        }
    }
}
