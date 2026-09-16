using AutoMapper;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Mappings
{
    public class RecentUsersProfile : Profile
    {
        public RecentUsersProfile()
        {
            CreateMap<User, RecentUsersResDto>();
        }
    }
}
