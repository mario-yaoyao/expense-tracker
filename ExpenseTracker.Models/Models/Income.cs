namespace ExpenseTracker.Models.Models
{
    public class Income : Base
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public bool IsDeleted { get; set; }

        public Category Category { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
