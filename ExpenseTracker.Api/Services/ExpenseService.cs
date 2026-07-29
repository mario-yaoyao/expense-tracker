using expense_tracker.Data;
using expense_tracker.Dtos.Requests;
using expense_tracker.Dtos.Responses;
using expense_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace expense_tracker.Services
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

        public async Task<ExpenseResDto?> CreateExpenseAsync(ExpenseReqDto expense)
        {
            var newExpense = new Expense
            {
                UserId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
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