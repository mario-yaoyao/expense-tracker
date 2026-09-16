using BudgetWise.Models.Models;

namespace BudgetWise.DAL.Interfaces
{
    public interface IProfileRepository
    {
        Task UpdatePasswordAsync(User request);
    }
}
