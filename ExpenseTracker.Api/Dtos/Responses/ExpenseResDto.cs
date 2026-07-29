namespace expense_tracker.Dtos.Responses
{
    public class ExpenseResDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
