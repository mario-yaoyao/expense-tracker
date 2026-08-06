using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.DAL.Data;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.DAL.Repositories
{
    public class AuthRepository(AppDbContext context, ILogger<AuthRepository> logger) : IAuthRepository
    {
        public async Task<User?> GetByUsernameAsync(string username)
        {
            try
            {
                return await context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                logger.LogError("Database error while fetching user by username: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            try
            {
                return await context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                logger.LogError("Database error while fetching user by ID: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            try
            {
                return await context.Users
                    .AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                logger.LogError("Database error while checking if username is taken: {Message}", ex.Message);
                throw;
            }
        }

        public async Task AddUserAsync(User user)
        {
            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Database error while adding user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError("Database error while saving auth changes: {Message}", ex.Message);
                throw;
            }
        }
    }
}
