using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IIncomeRepository
    {
        Task<(List<Income> data, int totalCount, bool hasNextPage)> GetAllIncomesAsync(int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null);
        Task<(List<Income> data, decimal totalIncome, HighestAmountResDto? highestIncome, int totalCount, bool hasNextPage)> GetIncomesByUserAsync(int userId, int page = 1, int limit = 20, string? search = null, DateOnly? startDate = null, DateOnly? endDate = null);
        Task<Income?> GetIncomeByUserAsync(int userId, int incomeId);
        Task<Income?> GetIncomeByIdAsync(int incomeId);
        Task AddIncomeAsync(Income income);
        Task SaveChangesAsync();
    }
}
