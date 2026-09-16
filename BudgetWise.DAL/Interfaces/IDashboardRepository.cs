using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.DAL.Interfaces
{
    public interface IDashboardRepository
    {
        Task<(SuperAdminDashboardMetricsResDto metrics, List<UserGrowthTrendResDto> userGrowthTrend, List<User> recentUsers, List<RecentTransactionsResDto> recentTransactions)> GetSuperAdminDashboardAsync();
        Task<(UserDashboardMetricsResDto metrics, List<SavingsTrendResDto> savingsTrend, List<IncomeExpenseTrendResDto> incomeExpenseTrend, List<RecentTransactionsResDto> recentTransactions)> GetUserDashboardAsync(int userId);
    }
}
