using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Models.Dtos.Responses
{
    public class SuperAdminDashboardResDto
    {
        public SuperAdminDashboardMetricsResDto Metrics { get; set; } = new();
        public List<UserGrowthTrendResDto> UsersGrowthTrend { get; set; } = [];
        public List<RecentUsersResDto> RecentUsers { get; set; } = [];
    }
}
