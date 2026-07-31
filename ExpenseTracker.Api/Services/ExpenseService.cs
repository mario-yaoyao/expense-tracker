using ExpenseTracker.Data;
using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services
{
    public class ExpenseService(AppDbContext context) : IExpenseService
    {
        public async Task<List<ExpenseResDto>> GetExpensesAsync(Guid userId, string role)
        {
            var query = context.Users
                .AsQueryable();

            if (role == "User")
            {
                query = query.Where(u => u.Id == userId);
            }

            var expenses = await query
                .SelectMany(u => u.Expenses)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
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

        public async Task<ExpenseResDto?> GetExpenseByIdAsync(Guid userId, string role, Guid expenseId)
        {
            var query = context.Users
                .AsQueryable();

            if (role == "User")
            {
                query = query.Where(u => u.Id == userId);
            }

            var existingExpense = await query
                //.Where(u => u.Id == userId)
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
                .Where(e => e.Id == expenseId)
                .FirstOrDefaultAsync();

            return existingExpense;
        }

        public async Task<ExpenseResDto?> CreateExpenseAsync(Guid userId, string role, ExpenseReqDto expense)
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

            return await GetExpenseByIdAsync(userId, role, newExpense.Id);
        }

        public async Task<ExpenseResDto?> UpdateExpenseAsync(Guid userId, string role, Guid expenseId, ExpenseReqDto expense)
        {
            var existingExpense = await context.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Expenses)
                .Where(e => e.Id == expenseId)
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

            return await GetExpenseByIdAsync(userId, role, expenseId);
        }

        //NOTE: permanent delete the expense from the database
        public async Task<bool> DeleteExpenseAsync(Guid userId, Guid expenseId)
        {
            var existingExpense = await context.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Expenses)
                .Where(e => e.Id == expenseId)
                .FirstOrDefaultAsync();

            if (existingExpense == null) return false;

            context.Expenses.Remove(existingExpense);
            await context.SaveChangesAsync();

            return true;
        }

        //NOTE: soft delete the expense by setting IsDeleted to true
        //public async Task<bool> DeleteExpenseAsync(Guid userId, Guid expenseId)
        //{
        //    // soft delete the expense by setting IsDeleted to true
        //    var existingExpense = await context.Users
        //        .Where(u => u.Id == userId)
        //        .SelectMany(u => u.Expenses)
        //        .Where(e => e.Id == expenseId)
        //        .FirstOrDefaultAsync();

        //    if (existingExpense == null) return false;

        //    existingExpense.IsDeleted = true;

        //    await context.SaveChangesAsync();

        //    return true;
        //}
    }
}