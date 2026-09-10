namespace ExpenseTracker.Models.Dtos.Responses
{
    public class UsersResDto
    {
        public List<UserResDto> Items { get; set; } = [];
        public PaginatedResDto Pagination { get; set; } = new();
    }
}
