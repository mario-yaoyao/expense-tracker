using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IProfileRepository
    {
        Task UpdatePasswordAsync(User request);
    }
}
