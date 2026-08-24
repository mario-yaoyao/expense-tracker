using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class UpdateCategoryReqDto
    {
        public string Name { get; set; } = string.Empty;
        public CategoryType? Type { get; set; }
    }
}
