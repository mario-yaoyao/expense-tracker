using AutoMapper;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;

namespace ExpenseTracker.BLL.Mappings
{
    public class IncomeProfile : Profile
    {
        public IncomeProfile()
        {
            CreateMap<Income, IncomeResDto>()
                .ForMember(
                    dest => dest.Username,
                    opt => opt.MapFrom(src => src.User.Username))
                .ForMember(
                    dest => dest.FullName,
                    opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(
                    dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name)
                )
                .ForMember(
                    dest => dest.CategoryType,
                    opt => opt.MapFrom(src => src.Category.Type)
                );
        }
    }
}
