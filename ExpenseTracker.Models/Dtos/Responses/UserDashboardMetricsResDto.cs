namespace ExpenseTracker.Models.Dtos.Responses
{
    public class UserDashboardMetricsResDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
    }
}
