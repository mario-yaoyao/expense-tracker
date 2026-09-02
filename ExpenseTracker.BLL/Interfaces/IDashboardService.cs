using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IDashboardService
    {
        Task<(SuperAdminDashboardMetricsResDto metrics, List<UserGrowthTrendResDto> usersGrowthTrend, List<RecentUsersResDto> recentUsers)> GetSuperAdminDashboardAsync();
        Task<(UserDashboardMetricsResDto metrics, List<SavingsTrendResDto> savingsTrend, List<IncomeExpenseTrendResDto> incomeExpenseTrend)> GetUserDashboardAsync(int userId);
    }
}
