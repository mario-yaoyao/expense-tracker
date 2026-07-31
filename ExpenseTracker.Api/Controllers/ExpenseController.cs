using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Services;
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
        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet]
        public async Task<ActionResult<List<ExpenseResDto>>> GetExpenses()
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await expenseService.GetExpensesAsync(userId, role);

                if (data.Count == 0) return NotFound(new ApiResDto<object>
                {
                    success = false,
                    message = "No expenses found."
                });

                return Ok(new ApiResDto<List<ExpenseResDto>>
                {
                    success = true,
                    data = data,
                    message = "Expenses retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = $"An error occurred while retrieving expenses. {ex.Message}"
                });
            }
        }

        [HttpGet("id")]
        public async Task<ActionResult<ApiResDto<ExpenseResDto>>> GetExpenseById(Guid expenseId)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await expenseService.GetExpenseByIdAsync(userId, role, expenseId);

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    success = false,
                    message = "Expense not found."
                });

                return Ok(new ApiResDto<ExpenseResDto>
                {
                    success = true,
                    data = data,
                    message = "Expense retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = $"An error occurred while retrieving the expense. {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResDto<ExpenseResDto>>> CreateExpense([FromBody] ExpenseReqDto expense)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await expenseService.CreateExpenseAsync(userId, role, expense);

                return Ok(new ApiResDto<ExpenseResDto>
                {
                    success = true,
                    data = data,
                    message = "Expense record created successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = $"An error occurred while creating the expense. {ex.Message}"
                });
            }
        }

        [HttpPut("id")]
        public async Task<ActionResult<ExpenseResDto>> UpdateExpense(Guid expenseId, [FromBody] ExpenseReqDto expense)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await expenseService.UpdateExpenseAsync(userId, role, expenseId, expense);

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    success = false,
                    message = "Expense not found."
                });

                return Ok(new ApiResDto<ExpenseResDto>
                {
                    success = true,
                    data = data,
                    message = "Expense record updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = $"An error occurred while updating the expense. {ex.Message}"
                });
            }
        }

        [HttpDelete("id")]
        public async Task<ActionResult<List<ExpenseResDto?>>> DeleteExpense(Guid expenseId)
        {
            try
            {
                var userId = GetUserId();
                var data = await expenseService.DeleteExpenseAsync(userId, expenseId);

                if (!data) return NotFound(new ApiResDto<object>
                {
                    success = false,
                    message = "Expense not found."
                });

                return Ok(new ApiResDto<ExpenseResDto>
                {
                    success = true,
                    message = "Expense record deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    success = false,
                    message = $"An error occurred while deleting the expense. {ex.Message}"
                });
            }
        }
    }
}
