namespace ExpenseTracker.Models.Dtos.Responses
{
    public class CategoriesResDto
    {
        public List<CategoryResDto> Items { get; set; } = [];
        public PaginatedResDto Pagination { get; set; } = new();
    }
}
