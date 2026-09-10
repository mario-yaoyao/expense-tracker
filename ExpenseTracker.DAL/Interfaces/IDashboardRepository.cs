using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.DAL.Interfaces
{
    public interface IDashboardRepository
    {
        Task<(SuperAdminDashboardMetricsResDto metrics, List<UserGrowthTrendResDto> userGrowthTrend, List<User> recentUsers, List<RecentTransactionsResDto> recentTransactions)> GetSuperAdminDashboardAsync();
        Task<(UserDashboardMetricsResDto metrics, List<SavingsTrendResDto> savingsTrend, List<IncomeExpenseTrendResDto> incomeExpenseTrend, List<RecentTransactionsResDto> recentTransactions)> GetUserDashboardAsync(int userId);
    }
}
