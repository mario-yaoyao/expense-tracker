using AutoMapper;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResDto>();
        }
    }
}
