using expense_tracker.Dtos;

namespace expense_tracker.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly List<ExpenseDto> _expenses = [
                new ExpenseDto
                {
                    Id = 1,
                    Description = "Expense #1",
                    Amount = "$100.00",
                    Category = "Category #1",
                    CreatedAt = "2023-01-01",
                    UpdatedAt = "2023-01-01"
                },
                new ExpenseDto
                {
                    Id = 2,
                    Description = "Expense #2",
                    Amount = "$200.00",
                    Category = "Category #2",
                    CreatedAt = "2023-01-02",
                    UpdatedAt = "2023-01-02"
                }
            ];

        public Task<List<ExpenseDto>> GetExpensesAsync()
        {
        return Task.FromResult(_expenses);
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
        {
            var expenses = await GetExpensesAsync();
            return expenses.FirstOrDefault(e => e.Id == id);
        }

        public async Task<ExpenseDto?> CreateExpenseAsync(ExpenseDto expense)
        {
            var expenses = await GetExpensesAsync();
            var newExpense = new ExpenseDto
            {
                Id = expenses.Max(e => e.Id) + 1,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd")
            };
            expenses.Add(newExpense);
            //Console.WriteLine(
            //    $"expenses: {string.Join(", ", expenses.Select(e => $"Id={e.Id}, Desc={e.Description}, Amount={e.Amount}"))}"
            //);
            return newExpense;
        }

        public async Task<ExpenseDto?> UpdateExpenseAsync(int id, ExpenseDto task)
        {
            var expenses = await GetExpensesAsync();
            var existingExpense = expenses.FirstOrDefault(e => e.Id == id);
            if (existingExpense == null) return null;

            existingExpense.Description = string.IsNullOrWhiteSpace(task.Description)
                ? existingExpense.Description
                : task.Description;

            existingExpense.Amount = string.IsNullOrWhiteSpace(task.Amount)
                ? existingExpense.Amount
                : task.Amount;

            existingExpense.Category = string.IsNullOrWhiteSpace(task.Category)
                ? existingExpense.Category
                : task.Category;

            existingExpense.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd");

            return existingExpense;
        }

        public async Task<List<ExpenseDto?>> DeleteExpenseAsync(int id)
        {
            var expenses = await GetExpensesAsync();
            var expenseToDelete = expenses.FirstOrDefault(e => e.Id == id);
            if (expenseToDelete == null) return null;
            expenses.Remove(expenseToDelete);

            var newExpenses = await GetExpensesAsync();
            return newExpenses;
        }
    }
}