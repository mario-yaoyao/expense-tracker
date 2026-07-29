namespace ExpenseTracker.Models
{
    public class User : Base
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public ICollection<Expense> Expenses { get; set; } = [];
    }
}
