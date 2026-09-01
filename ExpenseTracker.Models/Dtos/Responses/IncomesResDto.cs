namespace ExpenseTracker.Models.Dtos.Responses
{
    public class IncomesResDto
    {
        public List<IncomeResDto> Items { get; set; } = [];
        public FinancialMetricsResDto Metrics { get; set; } = new();
        public PaginatedResDto Pagination { get; set; } = new();
    }
}
