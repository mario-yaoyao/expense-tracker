using AutoMapper;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Services
{
    public class ExpenseService(IExpenseRepository expenseRepository, IMapper mapper) : IExpenseService
    {
        public async Task<(List<ExpenseResDto> data, decimal totalExpense, HighestAmountResDto? highestExpense, int totalCount, bool hasNextPage)> GetExpensesAsync(int userId, string role, ExpenseQueryReqDto request)
        {
            List<Expense> data;
            decimal totalExpense = 0;
            int totalCount;
            HighestAmountResDto? highestExpense = null;
            bool hasNextPage;

            if (role == "User")
            {
                (data, totalExpense, highestExpense, totalCount, hasNextPage) = await expenseRepository.GetExpensesByUserAsync(userId, request.Page, request.Limit, request.Search);
            }
            else
            {
                (data, totalCount, hasNextPage) = await expenseRepository.GetAllExpensesAsync(request.Page, request.Limit, request.Search);
            }

            return (
                mapper.Map<List<ExpenseResDto>>(data),
                totalExpense,
                highestExpense,
                totalCount,
                hasNextPage
            );
        }

        public async Task<ExpenseResDto?> GetExpenseByIdAsync(int userId, string role, int expenseId)
        {
            Expense? expense;

            if (role == "User")
            {
                expense = await expenseRepository.GetExpenseByUserAsync(userId, expenseId);
            }
            else
            {
                expense = await expenseRepository.GetExpenseByIdAsync(expenseId);
            }

            if (expense == null) return null;

            return mapper.Map<ExpenseResDto>(expense);
        }

        public async Task<ExpenseResDto?> CreateExpenseAsync(int userId, CreateExpenseReqDto expense)
        {
            var newExpense = new Expense
            {
                UserId = userId,
                Description = expense.Description,
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
            };

            await expenseRepository.AddExpenseAsync(newExpense);
            var createdExpense = await expenseRepository.GetExpenseByIdAsync(newExpense.Id);

            return mapper.Map<ExpenseResDto>(createdExpense);
        }

        public async Task<ExpenseResDto?> UpdateExpenseAsync(int userId, int expenseId, UpdateExpenseReqDto expense)
        {
            var existingExpense = await expenseRepository.GetExpenseByUserAsync(userId, expenseId);

            if (existingExpense == null) return null;

            existingExpense.Description = string.IsNullOrWhiteSpace(expense.Description)
                ? existingExpense.Description
                : expense.Description;

            existingExpense.Amount = expense.Amount ?? existingExpense.Amount;
            existingExpense.CategoryId = expense.CategoryId ?? existingExpense.CategoryId;
            existingExpense.UpdatedAt = DateTime.UtcNow;

            await expenseRepository.SaveChangesAsync();

            return mapper.Map<ExpenseResDto>(existingExpense);
        }

        public async Task<bool> DeleteExpenseAsync(int userId, int expenseId)
        {
            var existingExpense = await expenseRepository.GetExpenseByUserAsync(userId, expenseId);

            if (existingExpense == null) return false;

            existingExpense.IsDeleted = true;

            await expenseRepository.SaveChangesAsync();

            return true;
        }
    }
}