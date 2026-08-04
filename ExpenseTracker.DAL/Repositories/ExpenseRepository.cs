using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.DAL.Repositories
{
    public class ExpenseRepository(AppDbContext context) : IExpenseRepository
    {
        public async Task<List<Expense>> GetAllExpensesAsync()
        {
            return await context.Expenses
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Expense>> GetExpensesByUserAsync(Guid userId)
        {
            return await context.Expenses
                .Where(e => e.UserId == userId && !e.IsDeleted)
                .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Expense?> GetExpenseByUserAsync(Guid userId, Guid expenseId)
        {
            return await context.Expenses
                .Where(e => !e.IsDeleted)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == expenseId);
        }

        public async Task<Expense?> GetExpenseByIdAsync(Guid expenseId)
        {
            return await context.Expenses
                .FirstOrDefaultAsync(e => e.Id == expenseId && !e.IsDeleted);
        }

        public async Task AddExpenseAsync(Expense expense)
        {
            await context.Expenses.AddAsync(expense);
            await context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
