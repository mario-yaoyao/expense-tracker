using BudgetWise.DAL.Data;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Models;
using Microsoft.Extensions.Logging;

namespace BudgetWise.DAL.Repositories
{
    public class ProfileRepository(AppDbContext context, ILogger<ProfileRepository> logger) : IProfileRepository
    {
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
