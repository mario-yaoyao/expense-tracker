namespace expense_tracker.Dtos
{
    public class ExpenseDto
    {
        public int? Id { get; set; }
        public string Description { get; set; }
        public string Amount { get; set; }
        public string Category { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }
}
