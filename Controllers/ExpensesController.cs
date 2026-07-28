using expense_tracker.Dtos.Requests;
using expense_tracker.Dtos.Responses;
using expense_tracker.Models;
using expense_tracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [Route("api/expenses")]
    [ApiController]
    public class ExpensesController(IExpenseService expenseService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ExpenseResDto>>> GetExpenses()
        {
            try
            {
                var data = await expenseService.GetExpensesAsync();

                if (data.Count == 0) return NotFound("No expenses found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving expenses. {ex.Message}");
            }
        }

        [HttpGet("id")]
        public async Task<ActionResult<ExpenseResDto>> GetExpenseById(Guid id)
        {
            try
            {
                var data = await expenseService.GetExpenseByIdAsync(id);

                if (data == null) return NotFound("Expense not found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the expense. {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseResDto>> CreateExpense([FromBody] ExpenseReqDto expense)
        {
            try
            {
                var data = await expenseService.CreateExpenseAsync(expense);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the expense. {ex.Message}");
            }
        }

        [HttpPut("id")]
        public async Task<ActionResult<ExpenseResDto>> UpdateExpense(Guid id, [FromBody] ExpenseReqDto expense)
        {
            try
            {
                var data = await expenseService.UpdateExpenseAsync(id, expense);

                if (data == null) return NotFound("Expense not found.");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the expense. {ex.Message}");
            }
        }

        [HttpDelete("id")]
        public async Task<ActionResult<List<ExpenseResDto?>>> DeleteExpense(Guid id)
        {
            try
            {
                var data = await expenseService.DeleteExpenseAsync(id);

                if (!data) return NotFound("Expense not found.");

                return Ok("Expense deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the expense. {ex.Message}");
            }
        }
    }
}
