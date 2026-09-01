using AutoMapper;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Services
{
    public class IncomeService(IIncomeRepository incomeRepository, IMapper mapper) : IIncomeService
    {
        public async Task<(List<IncomeResDto> data, decimal totalIncome, HighestAmountResDto? highestIncome, int totalCount, bool hasNextPage)> GetIncomesAsync(int userId, string role, IncomeQueryReqDto request)
        {
            List<Income> data;
            decimal totalIncome = 0;
            int totalCount;
            HighestAmountResDto? highestIncome = null;
            bool hasNextPage;

            if (role == "User")
            {
                (data, totalIncome, highestIncome, totalCount, hasNextPage) = await incomeRepository.GetIncomesByUserAsync(userId, request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
            }
            else
            {
                (data, totalCount, hasNextPage) = await incomeRepository.GetAllIncomesAsync(request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
            }

            return (
                mapper.Map<List<IncomeResDto>>(data),
                totalIncome,
                highestIncome,
                totalCount,
                hasNextPage
            );
        }

        public async Task<IncomeResDto?> GetIncomeByIdAsync(int userId, string role, int incomeId)
        {
            Income? income;

            if (role == "User")
            {
                income = await incomeRepository.GetIncomeByUserAsync(userId, incomeId);
            }
            else
            {
                income = await incomeRepository.GetIncomeByIdAsync(incomeId);
            }

            if (income == null) return null;

            return mapper.Map<IncomeResDto>(income);
        }

        public async Task<IncomeResDto?> CreateIncomeAsync(int userId, CreateIncomeReqDto income)
        {
            var newIncome = new Income
            {
                UserId = userId,
                Description = income.Description,
                Amount = income.Amount,
                CategoryId = income.CategoryId,
            };

            await incomeRepository.AddIncomeAsync(newIncome);
            var createdIncome = await incomeRepository.GetIncomeByIdAsync(newIncome.Id);

            return mapper.Map<IncomeResDto>(createdIncome);
        }

        public async Task<IncomeResDto?> UpdateIncomeAsync(int userId, int incomeId, UpdateIncomeReqDto income)
        {
            var existingIncome = await incomeRepository.GetIncomeByUserAsync(userId, incomeId);

            if (existingIncome == null) return null;

            existingIncome.Description = string.IsNullOrWhiteSpace(income.Description)
                ? existingIncome.Description
                : income.Description;

            existingIncome.Amount = income.Amount ?? existingIncome.Amount;
            existingIncome.CategoryId = income.CategoryId ?? existingIncome.CategoryId;
            existingIncome.UpdatedAt = DateTime.UtcNow;

            await incomeRepository.SaveChangesAsync();

            return mapper.Map<IncomeResDto>(existingIncome);
        }

        public async Task<bool> DeleteIncomeAsync(int userId, int incomeId)
        {
            var existingIncome = await incomeRepository.GetIncomeByUserAsync(userId, incomeId);

            if (existingIncome == null) return false;

            existingIncome.IsDeleted = true;

            await incomeRepository.SaveChangesAsync();

            return true;
        }
    }
}
