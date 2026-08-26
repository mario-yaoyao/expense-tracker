using AutoMapper;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Mappings
{
    public class HighestExpenseProfile : Profile
    {
        public HighestExpenseProfile()
        {
            CreateMap<User, ProfileResDto>();
        }
    }
}
