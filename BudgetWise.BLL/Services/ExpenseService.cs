using AutoMapper;
using BudgetWise.BLL.Interfaces;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Serilog;

namespace BudgetWise.BLL.Services
{
    public class ExpenseService(IExpenseRepository expenseRepository, IUserRepository userRepository, IMapper mapper) : IExpenseService
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
                (data, totalExpense, highestExpense, totalCount, hasNextPage) = await expenseRepository.GetExpensesByUserAsync(userId, request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
            }
            else
            {
                (data, totalCount, hasNextPage) = await expenseRepository.GetAllExpensesAsync(request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
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

        public async Task<bool> CreateExpenseAsync(int userId, CreateExpenseReqDto expense)
        {
            var newExpense = new Expense
            {
                UserId = userId,
                Description = expense.Description,
                Amount = expense.Amount,
                CategoryId = expense.CategoryId,
            };

            await expenseRepository.AddExpenseAsync(newExpense);
            var user = await userRepository.GetUserByIdAsync(userId);

            Log.ForContext("IsAuditLog", true)
               .ForContext("UserId", userId)
               .ForContext("Username", user!.Username)
               .ForContext("Type", (int)TransactionType.Create)
               .ForContext("EntityName", "Expense")
               .ForContext("Activity", $"Created expense '{newExpense.Description}'.")
               .Information($"'{user.Username}' created expense '{newExpense.Description}'.");

            return true;
        }

        public async Task<bool> UpdateExpenseAsync(int userId, int expenseId, UpdateExpenseReqDto expense)
        {
            var existingExpense = await expenseRepository.GetExpenseByUserAsync(userId, expenseId);

            if (existingExpense == null) return false;

            existingExpense.Description = string.IsNullOrWhiteSpace(expense.Description)
                ? existingExpense.Description
                : expense.Description;

            existingExpense.Amount = expense.Amount ?? existingExpense.Amount;
            existingExpense.CategoryId = expense.CategoryId ?? existingExpense.CategoryId;
            existingExpense.UpdatedAt = DateTime.UtcNow;

            await expenseRepository.SaveChangesAsync();
            var user = await userRepository.GetUserByIdAsync(userId);

            Log.ForContext("IsAuditLog", true)
               .ForContext("UserId", userId)
               .ForContext("Username", user!.Username)
               .ForContext("Type", (int)TransactionType.Update)
               .ForContext("EntityName", "Expense")
               .ForContext("Activity", $"Updated expense '{existingExpense.Description}'.")
               .Information($"'{user.Username}' updated expense '{existingExpense.Description}'.");

            return true;
        }

        public async Task<bool> DeleteExpenseAsync(int userId, int expenseId)
        {
            var existingExpense = await expenseRepository.GetExpenseByUserAsync(userId, expenseId);

            if (existingExpense == null) return false;

            existingExpense.IsDeleted = true;

            await expenseRepository.SaveChangesAsync();
            var user = await userRepository.GetUserByIdAsync(userId);

            Log.ForContext("IsAuditLog", true)
               .ForContext("UserId", userId)
               .ForContext("Username", user!.Username)
               .ForContext("Type", (int)TransactionType.Delete)
               .ForContext("EntityName", "Expense")
               .ForContext("Activity", $"Deleted expense '{existingExpense.Description}'.")
               .Information($"'{user.Username}' deleted expense '{existingExpense.Description}'.");

            return true;
        }
    }
}