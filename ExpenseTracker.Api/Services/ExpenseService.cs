using ExpenseTracker.Data;
using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class ExpenseService(AppDbContext context) : IExpenseService
    {
        public async Task<List<ExpenseResDto>> GetExpensesAsync()
        {
            var expenses = await context.Users
                .SelectMany(u => u.Expenses)
                .Where(e => !e.IsDeleted)
                .Select(e => new ExpenseResDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    Description = e.Description,
                    Amount = e.Amount,
                    Category = e.Category,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync();

            return expenses;
        }

        public async Task<ExpenseResDto?> GetExpenseByIdAsync(Guid id)
        {
            var existingExpense = await context.Users
                .SelectMany(u => u.Expenses)
                .Select(e => new ExpenseResDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    Description = e.Description,
                    Amount = e.Amount,
                    Category = e.Category,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            return existingExpense;
        }

        public async Task<ExpenseResDto?> CreateExpenseAsync(Guid userId, ExpenseReqDto expense)
        {
            var newExpense = new Expense
            {
                UserId = userId,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category,
            };

            context.Expenses.Add(newExpense);
            await context.SaveChangesAsync();

            return await GetExpenseByIdAsync(newExpense.Id);
        }

        public async Task<ExpenseResDto?> UpdateExpenseAsync(Guid id, ExpenseReqDto expense)
        {
            var existingExpense = await context.Users
                .SelectMany(u => u.Expenses)
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (existingExpense == null) return null;

            existingExpense.Description = string.IsNullOrWhiteSpace(expense.Description)
                ? existingExpense.Description
                : expense.Description;

            existingExpense.Amount = expense.Amount;

            existingExpense.Category = string.IsNullOrWhiteSpace(expense.Category)
                ? existingExpense.Category
                : expense.Category;

            await context.SaveChangesAsync();

            return await GetExpenseByIdAsync(id);
        }

        //NOTE: permanent delete the expense from the database
        public async Task<bool> DeleteExpenseAsync(Guid id)
        {
            var existingExpense = await context.Users
                .SelectMany(u => u.Expenses)
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (existingExpense == null) return false;

            context.Expenses.Remove(existingExpense);
            await context.SaveChangesAsync();

            return true;
        }

        //NOTE: soft delete the expense by setting IsDeleted to true
        //public async Task<bool> DeleteExpenseAsync(Guid id)
        //{
        //    // soft delete the expense by setting IsDeleted to true
        //    var existingExpense = await context.Users
        //        .SelectMany(u => u.Expenses)
        //        .Where(e => e.Id == id)
        //        .FirstOrDefaultAsync();

        //    if (existingExpense == null) return false;

        //    existingExpense.IsDeleted = true;

        //    await context.SaveChangesAsync();

        //    return true;
        //}
    }
}