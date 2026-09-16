
using AutoMapper;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;

namespace BudgetWise.BLL.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryResDto>();
        }
    }
}
