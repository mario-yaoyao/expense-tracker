using AutoMapper;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Services
{
    public class DashboardService(IDashboardRepository dashboardRepository, IMapper mapper) : IDashboardService
    {
        public async Task<(SuperAdminDashboardMetricsResDto metrics, List<UserGrowthTrendResDto> usersGrowthTrend, List<RecentUsersResDto> recentUsers, List<RecentTransactionsResDto> recentTransactions)> GetSuperAdminDashboardAsync()
        {
            SuperAdminDashboardMetricsResDto metrics;
            List<UserGrowthTrendResDto> usersGrowthTrend;
            List<User> recentUsers;
            List<RecentTransactionsResDto> recentTransactions;

            (metrics, usersGrowthTrend, recentUsers, recentTransactions) = await dashboardRepository.GetSuperAdminDashboardAsync();

            return (
                metrics,
                usersGrowthTrend,
                mapper.Map<List<RecentUsersResDto>>(recentUsers),
                recentTransactions
            );
        }

        public async Task<(UserDashboardMetricsResDto metrics, List<SavingsTrendResDto> savingsTrend, List<IncomeExpenseTrendResDto> incomeExpenseTrend, List<RecentTransactionsResDto> recentTransactions)> GetUserDashboardAsync(int userId)
        {
            UserDashboardMetricsResDto metrics;
            List<SavingsTrendResDto> savingsTrend;
            List<IncomeExpenseTrendResDto> incomeExpenseTrend;
            List<RecentTransactionsResDto> recentTransactions;

            (metrics, savingsTrend, incomeExpenseTrend, recentTransactions) = await dashboardRepository.GetUserDashboardAsync(userId);

            return (
                metrics,
                savingsTrend,
                incomeExpenseTrend,
                recentTransactions
            );
        }
    }
}
   

