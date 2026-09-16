using AutoMapper;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Mappings
{
    public class HighestExpenseProfile : Profile
    {
        public HighestExpenseProfile()
        {
            CreateMap<User, UserResDto>();
        }
    }
}
