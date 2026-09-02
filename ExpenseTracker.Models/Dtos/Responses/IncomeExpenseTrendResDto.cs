namespace ExpenseTracker.Models.Dtos.Responses
{
    public class IncomeExpenseTrendResDto
    {
        public string Month { get; set; } = string.Empty;

        public decimal Income { get; set; }

        public decimal Expense { get; set; }
    }
}
