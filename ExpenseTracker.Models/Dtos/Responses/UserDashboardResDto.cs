namespace ExpenseTracker.Models.Dtos.Responses
{
    public class UserDashboardResDto
    {
        public UserDashboardMetricsResDto Metrics { get; set; } = new();
        public List<SavingsTrendResDto> SavingsTrend { get; set; } = [];
        public List<IncomeExpenseTrendResDto> IncomeExpenseTrend { get; set; } = [];
        //public List<RecentTransactionsResDto> RecentTransactions { get; set; } = [];
    }
}
