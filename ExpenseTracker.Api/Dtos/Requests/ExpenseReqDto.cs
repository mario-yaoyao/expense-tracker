namespace ExpenseTracker.Dtos.Requests
{
    public class ExpenseReqDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
