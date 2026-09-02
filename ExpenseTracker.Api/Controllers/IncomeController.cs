using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    [Route("api/incomes")]
    [ApiController]
    public class IncomeController(IIncomeService incomeService) : ControllerBase
    {
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet]
        public async Task<ActionResult<ApiResDto<IncomesResDto>>> GetIncomes([FromQuery] IncomeQueryReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var (data, totalIncome, highestIncome, totalCount, hasNextPage) = await incomeService.GetIncomesAsync(userId, role, request);

                return Ok(new ApiResDto<IncomesResDto>
                {
                    Success = true,
                    Data = new IncomesResDto
                    {
                        Items = data,
                        Metrics = new FinancialMetricsResDto
                        {
                            TotalAmount = totalIncome,
                            TotalCount = totalCount,
                            HighestAmount = highestIncome
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
                    ErrorMessage = "An error occurred while retrieving incomes."
                });
            }
        }

        [HttpGet("{incomeId}")]
        public async Task<ActionResult<ApiResDto<IncomeResDto>>> GetIncomeById(int incomeId)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await incomeService.GetIncomeByIdAsync(userId, role, incomeId);

                return data == null
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Income not found."
                    })
                    : Ok(new ApiResDto<IncomeResDto>
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
                    ErrorMessage = "An error occurred while retrieving the income."
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResDto<IncomeResDto>>> CreateIncome([FromBody] CreateIncomeReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var role = GetRole();
                var data = await incomeService.CreateIncomeAsync(userId, request);

                return Ok(new ApiResDto<IncomeResDto>
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
                    ErrorMessage = "An error occurred while creating the income."
                });
            }
        }

        [HttpPatch("{incomeId}")]
        public async Task<ActionResult<ApiResDto<IncomeResDto>>> UpdateIncome(int incomeId, [FromBody] UpdateIncomeReqDto request)
        {
            try
            {
                var userId = GetUserId();
                var data = await incomeService.UpdateIncomeAsync(userId, incomeId, request);

                return data == null
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Income not found."
                    })
                    : Ok(new ApiResDto<IncomeResDto>
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
                    ErrorMessage = "An error occurred while updating the income."
                });
            }
        }

        [HttpDelete("{incomeId}")]
        public async Task<ActionResult<ApiResDto<object>>> DeleteIncome(int incomeId)
        {
            try
            {
                var userId = GetUserId();
                var data = await incomeService.DeleteIncomeAsync(userId, incomeId);

                return !data
                    ? NotFound(new ApiResDto<object>
                    {
                        Success = false,
                        ErrorMessage = "Income not found."
                    })
                    : Ok(new ApiResDto<object>
                    {
                        Success = true,
                    });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResDto<object>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while deleting the income."
                });
            }
        }
    }
}
