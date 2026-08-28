namespace ExpenseTracker.Models.Dtos.Responses
{
    public class ExpensesResDto
    {
        public List<ExpenseResDto> Items { get; set; } = [];
        public FinancialMetricsResDto Metrics { get; set; } = new();
        public PaginatedResDto Pagination { get; set; } = new();
    }
}
