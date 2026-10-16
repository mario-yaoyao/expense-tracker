namespace BudgetWise.Models.Models
{
    public class RecurringTransaction : Base
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public CategoryType Type { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Expense> Expenses { get; set; } = [];
        public ICollection<Income> Incomes { get; set; } = [];
    }
}
