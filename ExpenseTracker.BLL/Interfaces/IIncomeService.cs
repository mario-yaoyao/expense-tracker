using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IIncomeService
    {
        Task<(List<IncomeResDto> data, decimal totalIncome, HighestAmountResDto? highestIncome, int totalCount, bool hasNextPage)> GetIncomesAsync(int userId, string role, IncomeQueryReqDto request);
        Task<IncomeResDto?> GetIncomeByIdAsync(int userId, string role, int id);
        Task<IncomeResDto?> CreateIncomeAsync(int userId, CreateIncomeReqDto income);
        Task<IncomeResDto?> UpdateIncomeAsync(int userId, int id, UpdateIncomeReqDto income);
        Task<bool> DeleteIncomeAsync(int userId, int id);
    }
}
