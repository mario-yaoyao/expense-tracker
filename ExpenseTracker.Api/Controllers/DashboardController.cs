using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController(IDashboardService dashboardService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("super-admin")]
        public async Task<ActionResult<ApiResDto<SuperAdminDashboardResDto>>> GetSuperAdminDashboard()
        {
            try
            {
                var (metrics, usersGrowthTrend, recentUsers) = await dashboardService.GetSuperAdminDashboardAsync();

                return Ok(new ApiResDto<SuperAdminDashboardResDto>
                {
                    Success = true,
                    Data = new SuperAdminDashboardResDto
                    {
                        Metrics = metrics,
                        UsersGrowthTrend = usersGrowthTrend,
                        RecentUsers = recentUsers
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving dashboard record."
                });
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("user")]
        public async Task<ActionResult<ApiResDto<UserDashboardResDto>>> GetUserDashboard()
        {
            try
            {
                var userId = GetUserId();
                var (metrics, savingsTrend, incomeExpenseTrend) = await dashboardService.GetUserDashboardAsync(userId);

                return Ok(new ApiResDto<UserDashboardResDto>
                {
                    Success = true,
                    Data = new UserDashboardResDto
                    {
                        Metrics = metrics,
                        SavingsTrend = savingsTrend,
                        IncomeExpenseTrend = incomeExpenseTrend
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving dashboard record."
                });
            }
        }
    }
}
