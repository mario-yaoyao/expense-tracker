using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [Route("api/expenses")]
    [ApiController]
    public class ExpenseController(IExpenseService expenseService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet]
        public async Task<ActionResult<ApiResDto<ExpensesResDto>>> GetExpenses([FromQuery] ExpenseQueryReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var (data, totalExpense, highestExpense, totalCount, hasNextPage) = await expenseService.GetExpensesAsync(userId, role, request);

                return Ok(new ApiResDto<ExpensesResDto>
                {
                    Success = true,
                    Data = new ExpensesResDto
                    {
                        Items = data,
                        Metrics = new FinancialMetricsResDto
                        {
                            TotalAmount = totalExpense,
                            TotalCount = totalCount,
                            HighestAmount= highestExpense
                        },
                        Pagination = new PaginatedResDto
                        {
                            Page = request.Page,
                            Limit = request.Limit,
                            HasNextPage = hasNextPage
                        }
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving expenses."
                });
            }
        }

        [HttpGet("{expenseId}")]
        public async Task<ActionResult<ApiResDto<ExpenseResDto>>> GetExpenseById(int expenseId)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await expenseService.GetExpenseByIdAsync(userId, role, expenseId);

                return data == null
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Expense not found."
                    })
                    : Ok(new ApiResDto<ExpenseResDto>
                    {
                        Success = true,
                        Data = data
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving the expense."
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResDto<ExpenseResDto>>> CreateExpense([FromBody] CreateExpenseReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await expenseService.CreateExpenseAsync(userId, request);

                return Ok(new ApiResDto<ExpenseResDto>
                {
                    Success = true,
                    Data = data,
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating the expense."
                });
            }
        }

        [HttpPatch("{expenseId}")]
        public async Task<ActionResult<ApiResDto<ExpenseResDto>>> UpdateExpense(int expenseId, [FromBody] UpdateExpenseReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var data = await expenseService.UpdateExpenseAsync(userId, expenseId, request);

                return data == null
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Expense not found."
                    })
                    : Ok(new ApiResDto<ExpenseResDto>
                    {
                        Success = true,
                        Data = data
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the expense."
                });
            }
        }

        [HttpDelete("{expenseId}")]
        public async Task<ActionResult<ApiResDto<object>>> DeleteExpense(int expenseId)
        {
            try
            {
                var userId = GetUserId();
                var data = await expenseService.DeleteExpenseAsync(userId, expenseId);

                return !data
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Expense not found."
                    })
                    : Ok(new ApiResDto<object>
                    {
                        Success = true,
                    });
            }
            catch (Exception )
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while deleting the expense."
                });
            }
        }
    }
}
