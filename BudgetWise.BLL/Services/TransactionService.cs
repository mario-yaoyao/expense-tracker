using AutoMapper;
using BudgetWise.BLL.Interfaces;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Services
{
    public class TransactionService(ITransactionRepository transactionRepository, IMapper mapper) : ITransactionService
    {
        public async Task<(List<TransactionResDto> data, bool hasNextPage)> GetTransactionsAsync(int userId, string role, TransactionQueryReqDto request)
        {
            List<Transaction> data;
            bool hasNextPage;

            if (role == "User")
            {
                (data, hasNextPage) = await transactionRepository.GetTransactionsByUserAsync(userId, request.Type, request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
            }
            else
            {
                (data, hasNextPage) = await transactionRepository.GetAllTransactionsAsync(request.Type, request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);
            }

            return (
                mapper.Map<List<TransactionResDto>>(data),
                hasNextPage
            );
        }

        public async Task<TransactionResDto?> GetTransactionByIdAsync(int userId, string role, int transactionId)
        {
            Transaction? category;

            if (role == "User")
            {
                category = await transactionRepository.GetTransactionAsync(userId, transactionId);
            }
            else
            {
                category = await transactionRepository.GetOwnTransactionAsync(transactionId);
            }

            if (category == null) return null;

            return mapper.Map<TransactionResDto>(category);
        }
    }
}
