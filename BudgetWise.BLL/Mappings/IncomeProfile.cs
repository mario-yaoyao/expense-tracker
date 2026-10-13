using AutoMapper;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Mappings
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
