using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ExpenseTracker.DAL.Repositories
{
    public class DashboardRepository(AppDbContext context, ILogger<ExpenseRepository> logger) : IDashboardRepository
    {
        public async Task<(SuperAdminDashboardMetricsResDto metrics, List<UserGrowthTrendResDto> userGrowthTrend, List<User> recentUsers)> GetSuperAdminDashboardAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var currentYear = currentDate.Year;
                var previousMonth = currentDate.AddMonths(-1);

                var metrics = await GetSuperAdminDashboardMetricsAsync(context);

                var monthlyUsers = await context.Users
                    .Where(u => u.IsActive &&
                                u.CreatedAt.Year == currentYear)
                    .GroupBy(u => u.CreatedAt.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var userGrowthTrend = BuildUserGrowthTrend(monthlyUsers);

                var recentUsers = await context.Users
                    .Where(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                return (metrics, userGrowthTrend, recentUsers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving dashboard records: {Message}", ex.Message);
                throw;
            }
        }

        //public async Task<(DashboardMetricsResDto metrics, List<SavingsTrendResDto> savingsTrend, List<IncomeExpenseTrendResDto> incomeExpenseTrend, List<RecentTransactionsResDto> recentTransactions)> GetDashboardByUserAsync(int userId)
        public async Task<(UserDashboardMetricsResDto metrics, List<SavingsTrendResDto> savingsTrend, List<IncomeExpenseTrendResDto> incomeExpenseTrend)> GetUserDashboardAsync(int userId)
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var currentYear = currentDate.Year;
                var previousMonth = currentDate.AddMonths(-1);

                var metrics = await GetUserDashboardMetricsAsync(context, userId);

                var monthlyIncome = await context.Incomes
                    .Where(i => i.UserId == userId &&
                                !i.IsDeleted &&
                                i.CreatedAt.Year == currentYear)
                    .GroupBy(i => i.CreatedAt.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        TotalIncome = g.Sum(i => i.Amount)
                    })
                    .ToListAsync();

                var monthlyExpense = await context.Expenses
                    .Where(e => e.UserId == userId &&
                                !e.IsDeleted &&
                                e.CreatedAt.Year == currentYear)
                    .GroupBy(e => e.CreatedAt.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        TotalExpense = g.Sum(e => e.Amount)
                    })
                    .ToListAsync();

                var savingsTrend = BuildSavingsTrend(monthlyIncome, monthlyExpense);
                var incomeExpenseTrend = BuildIncomeExpenseTrend(previousMonth.Month, monthlyIncome, monthlyExpense);

                //return (metrics, savingsTrend, incomeExpenseTrend, recentTransactions);
                return (metrics, savingsTrend, incomeExpenseTrend);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database error while retrieving dashboard records for user: {Message}", ex.Message);
                throw;
            }
        }

        // Helper Functions
        private static string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture
                .DateTimeFormat
                .GetAbbreviatedMonthName(month);
        }

        private static async Task<UserDashboardMetricsResDto> GetUserDashboardMetricsAsync(
            AppDbContext context,
            int userId)
        {
            var totalIncome = await context.Incomes
                .Where(i => i.UserId == userId && !i.IsDeleted)
                .SumAsync(i => i.Amount);

            var totalExpense = await context.Expenses
                .Where(e => e.UserId == userId && !e.IsDeleted)
                .SumAsync(e => e.Amount);

            return new UserDashboardMetricsResDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome - totalExpense
            };
        }

        private static async Task<SuperAdminDashboardMetricsResDto> GetSuperAdminDashboardMetricsAsync(
            AppDbContext context)
        {
            var currentDate = DateTime.UtcNow;
            var previousMonth = currentDate.AddMonths(-1);

            return new SuperAdminDashboardMetricsResDto
            {
                TotalUsers = await context.Users
                    .CountAsync(),
                ActiveUsers = await context.Users
                    .Where(u => u.IsActive)
                    .CountAsync(),
                NewUsers = await context.Users
                    .Where(u => u.CreatedAt >= previousMonth)
                    .CountAsync()
            };
        }

        private static (decimal Income, decimal Expense) GetMonthlySummary(
            int month,
            IEnumerable<dynamic> monthlyIncome,
            IEnumerable<dynamic> monthlyExpense)
        {
            var income = monthlyIncome
                .FirstOrDefault(x => x.Month == month)?.TotalIncome ?? 0;

            var expense = monthlyExpense
                .FirstOrDefault(x => x.Month == month)?.TotalExpense ?? 0;

            return (income, expense);
        }

        private static List<UserGrowthTrendResDto> BuildUserGrowthTrend(
            IEnumerable<dynamic> monthlyUsers)
        {
            return Enumerable.Range(1, 12)
                .Select(month => new UserGrowthTrendResDto
                {
                    Month = GetMonthName(month),
                    NewUsers = monthlyUsers
                        .FirstOrDefault(x => x.Month == month)
                        ?.Count ?? 0
                })
                .ToList();
        }

        private static List<SavingsTrendResDto> BuildSavingsTrend(
            IEnumerable<dynamic> monthlyIncome,
            IEnumerable<dynamic> monthlyExpense)
        {
            return Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var (income, expense) =
                        GetMonthlySummary(month, monthlyIncome, monthlyExpense);

                    return new SavingsTrendResDto
                    {
                        Month = GetMonthName(month),
                        Savings = income - expense
                    };
                })
                .ToList();
        }

        private static List<IncomeExpenseTrendResDto> BuildIncomeExpenseTrend(
            int previousMonth,
            IEnumerable<dynamic> monthlyIncome,
            IEnumerable<dynamic> monthlyExpense)
        {
            return Enumerable.Range(previousMonth, 2)
                .Select(month =>
                {
                    var (income, expense) =
                        GetMonthlySummary(month, monthlyIncome, monthlyExpense);

                    return new IncomeExpenseTrendResDto
                    {
                        Month = GetMonthName(month),
                        Income = income,
                        Expense = expense
                    };
                })
                .ToList();
        }
    }
}
