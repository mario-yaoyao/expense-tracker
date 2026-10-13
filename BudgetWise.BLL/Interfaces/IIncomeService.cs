using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;

namespace BudgetWise.BLL.Interfaces
{
    public interface IIncomeService
    {
        Task<(List<IncomeResDto> data, decimal totalIncome, HighestAmountResDto? highestIncome, int totalCount, bool hasNextPage)> GetIncomesAsync(int userId, string role, IncomeQueryReqDto request);
        Task<IncomeResDto?> GetIncomeByIdAsync(int userId, string role, int id);
        Task<bool> CreateIncomeAsync(int userId, CreateIncomeReqDto income);
        Task<bool> UpdateIncomeAsync(int userId, int id, UpdateIncomeReqDto income);
        Task<bool> DeleteIncomeAsync(int userId, int id);
    }
}
