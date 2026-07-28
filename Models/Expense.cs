using Microsoft.AspNetCore.Mvc.RazorPages;

namespace expense_tracker.Models
{
    public class Expense : Base
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsDeleted { get; set; }
    }
}
