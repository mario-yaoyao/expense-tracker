namespace BudgetWise.Models.Dtos.Responses
{
    public class SavingsTrendResDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Savings { get; set; }
    }
}
