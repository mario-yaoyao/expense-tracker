namespace ExpenseTracker.Models.Dtos.Requests
{
    public class UpdateExpenseReqDto
    {
        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Category { get; set; } = string.Empty;
    }
}
