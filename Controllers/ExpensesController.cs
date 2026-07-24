using expense_tracker.Dtos;
using expense_tracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace expense_tracker.Controllers
{
    [Route("api/expenses")]
    [ApiController]
    public class ExpensesController(IExpenseService expenseService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetExpenses()
        {
            try
            {
                var expenses = await expenseService.GetExpensesAsync();
                return Ok(expenses);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving expenses.");
            }
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetExpenseById(int id)
        {
            try
            {
                var expense = await expenseService.GetExpenseByIdAsync(id);
                return Ok(expense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the expense.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] ExpenseDto expense)
        {
            try
            {
                var createdExpense = await expenseService.CreateExpenseAsync(expense);
                return Ok(createdExpense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while creating the expense.");
            }
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseDto expense)
        {
            try
            {
                var updatedExpense = await expenseService.UpdateExpenseAsync(id, expense);
                return Ok(updatedExpense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the expense.");
            }
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var expenses = await expenseService.DeleteExpenseAsync(id);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the expense.");
            }
        }
    }
}
