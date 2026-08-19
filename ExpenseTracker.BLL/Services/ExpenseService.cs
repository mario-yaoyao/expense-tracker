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
        public async Task<List<ExpenseResDto>> GetExpensesAsync(int userId, string role)
        {
            List<Expense> expenses;

            if (role == "User")
            {
                expenses = await expenseRepository.GetExpensesByUserAsync(userId);
            }
            else
            {
                expenses = await expenseRepository.GetAllExpensesAsync();
            }

            return mapper.Map<List<ExpenseResDto>>(expenses);
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

            return mapper.Map<ExpenseResDto>(newExpense);
        }

        public async Task<ExpenseResDto?> UpdateExpenseAsync(int userId, string role, int expenseId, UpdateExpenseReqDto expense)
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

        ////NOTE: permanent delete the expense from the database
        //public async Task<bool> DeleteExpenseAsync(Guid userId, string role, Guid expenseId)
        //{
        //    var existingExpense = await expenseRepository.GetExpenseByIdAsync(userId, role, expenseId);

        //    if (existingExpense == null) return false;

        //    context.Expenses.Remove(existingExpense);
        //    await expenseRepository.SaveChangesAsync();

        //    return true;
        //}

        //NOTE: soft delete the expense by setting IsDeleted to true
        public async Task<bool> DeleteExpenseAsync(int userId, string role, int expenseId)
        {
            var existingExpense = await expenseRepository.GetExpenseByUserAsync(userId, expenseId);

            if (existingExpense == null) return false;

            existingExpense.IsDeleted = true;

            await expenseRepository.SaveChangesAsync();

            return true;
        }
    }
}