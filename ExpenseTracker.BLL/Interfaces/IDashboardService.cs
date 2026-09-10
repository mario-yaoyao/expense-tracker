using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Interfaces
{
    public interface IDashboardService
    {
        Task<(SuperAdminDashboardMetricsResDto metrics, List<UserGrowthTrendResDto> usersGrowthTrend, List<RecentUsersResDto> recentUsers, List<RecentTransactionsResDto> recentTransactions)> GetSuperAdminDashboardAsync();

        Task<(UserDashboardMetricsResDto metrics, List<SavingsTrendResDto> savingsTrend, List<IncomeExpenseTrendResDto> incomeExpenseTrend, List<RecentTransactionsResDto> recentTransactions)> GetUserDashboardAsync(int userId);
    }
}
