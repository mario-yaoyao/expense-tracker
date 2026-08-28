namespace ExpenseTracker.Models.Dtos.Responses
{
    public class FinancialMetricsResDto
    {
        public decimal? TotalAmount { get; set; }
        public int? TotalCount { get; set; }
        public HighestAmountResDto? HighestAmount { get; set; }
    }
}
