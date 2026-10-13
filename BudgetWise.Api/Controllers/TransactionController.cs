using BudgetWise.BLL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BudgetWise.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    public class TransactionController(ITransactionService transactionsService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet]
        public async Task<ActionResult<ApiResDto<TransactionsResDto>>> GetTransactions([FromQuery] TransactionQueryReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var (data, hasNextPage) = await transactionsService.GetTransactionsAsync(userId, role, request);

                return Ok(new ApiResDto<TransactionsResDto>
                {
                    Success = true,
                    Data = new TransactionsResDto
                    {
                        Items = data,
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
                    ErrorMessage = "An error occurred while retrieving transactions."
                });
            }
        }

        [HttpGet("{transactionId}")]
        public async Task<ActionResult<ApiResDto<TransactionResDto>>> GetTransactionById(int transactionId)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await transactionsService.GetTransactionByIdAsync(userId, role, transactionId);

                if (data == null) return NotFound(new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "Transaction not found."
                });

                return Ok(new ApiResDto<TransactionResDto>
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
                    ErrorMessage = "An error occurred while retrieving the transaction."
                });
            }
        }
    }
}
