using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;

namespace BudgetWise.BLL.Interfaces
{
    public interface ITransactionService
    {
        Task<(List<TransactionResDto> data, bool hasNextPage)> GetTransactionsAsync(int userId, string role, TransactionQueryReqDto request);
        Task<TransactionResDto?> GetTransactionByIdAsync(int userId, string role, int transactionId);
    }
}
