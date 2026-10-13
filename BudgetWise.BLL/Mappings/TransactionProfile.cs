using AutoMapper;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Mappings
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, TransactionResDto>();
        }
    }
}
