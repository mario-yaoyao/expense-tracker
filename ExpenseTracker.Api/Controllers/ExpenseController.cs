using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers
{
    [Authorize]
    [Route("api/expenses")]
    [ApiController]
    public class ExpenseController(IExpenseService expenseService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet]
        public async Task<ActionResult<List<ExpenseResDto>>> GetExpenses([FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] string? search = null)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var result = await expenseService.GetExpensesAsync(userId, role, page, limit, search);

                if (result.totalCount == 0) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "No expenses found."
                });

                return Ok(new ApiResDto<List<ExpenseResDto>>
                {
                    Success = true,
                    Data = result.data,
                    TotalExpense = result.totalExpense,
                    HighestExpense = result.highestExpense,
                    TotalCount = result.totalCount,
                    Page = page,
                    Limit = limit,
                    HasNextPage = result.hasNextPage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while retrieving expenses. {ex.Message}"
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

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "Expense not found."
                });

                return Ok(new ApiResDto<ExpenseResDto>
                {
                    Success = true,
                    Data = data,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while retrieving the expense. {ex.Message}"
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
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while creating the expense. {ex.Message}"
                });
            }
        }

        [HttpPut("{expenseId}")]
        public async Task<ActionResult<ExpenseResDto>> UpdateExpense(int expenseId, [FromBody] UpdateExpenseReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var data = await expenseService.UpdateExpenseAsync(userId, expenseId, request);

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "Expense not found."
                });

                return Ok(new ApiResDto<ExpenseResDto>
                {
                    Success = true,
                    Data = data,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while updating the expense. {ex.Message}"
                });
            }
        }

        [HttpDelete("{expenseId}")]
        public async Task<ActionResult<ExpenseResDto?>> DeleteExpense(int expenseId)
        {
            try
            {
                var userId = GetUserId();
                var data = await expenseService.DeleteExpenseAsync(userId, expenseId);

                if (!data) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "Expense not found."
                });

                return Ok(new ApiResDto<ExpenseResDto> // NOTE: only return ApiResDto in this scenario?
                {
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while deleting the expense. {ex.Message}"
                });
            }
        }
    }
}
