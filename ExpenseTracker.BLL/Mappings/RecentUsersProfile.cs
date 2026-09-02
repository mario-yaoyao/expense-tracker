using AutoMapper;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Mappings
{
    public class RecentUsersProfile : Profile
    {
        public RecentUsersProfile()
        {
            CreateMap<User, RecentUsersResDto>();
        }
    }
}
