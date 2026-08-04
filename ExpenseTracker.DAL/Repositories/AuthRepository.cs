using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.DAL.Data;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.DAL.Repositories
{
    public class AuthRepository(AppDbContext context) : IAuthRepository
    {
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> IsUsernameTakenAsync(string username) =>
            await context.Users.AnyAsync(u => u.Username == username);

        public async Task AddUserAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
