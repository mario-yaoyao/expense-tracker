using System.Runtime.InteropServices;

namespace ExpenseTracker.Models.Models
{
    public enum CategoryType
    {
        Expense,
        Income
    }

    public class Category : Base
    {
        public string Name { get; set; } = string.Empty;
        public CategoryType Type { get; set; }
        public bool IsDeleted { get; set; }
        public int UserId { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Expense> Expenses { get; set; } = [];
        public ICollection<Income> Incomes { get; set; } = [];
    }
}