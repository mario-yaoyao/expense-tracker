namespace ExpenseTracker.Models.Dtos.Responses
{
    public class UserGrowthTrendResDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal NewUsers { get; set; }
    }
}
