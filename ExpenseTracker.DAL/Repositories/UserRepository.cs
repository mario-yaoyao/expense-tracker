using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.DAL.Repositories
{
    public class UserRepository(AppDbContext context, ILogger<UserRepository> logger) : IUserRepository
    {
        public async Task<User?> GetUserProfileAsync(Guid userId)
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
    }
}
