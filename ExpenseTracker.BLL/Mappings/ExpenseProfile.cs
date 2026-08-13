using AutoMapper;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;


namespace ExpenseTracker.BLL.Mappings
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            CreateMap<Expense, ExpenseResDto>()
                .ForMember(
                    dest => dest.Username,
                    opt => opt.MapFrom(src => src.User.Username))
                .ForMember(
                    dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}
