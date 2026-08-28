namespace ExpenseTracker.Models.Dtos.Responses
{
    public class PaginatedResDto
    {
        public int? Page { get; set; }
        public int? Limit { get; set; }
        public bool? HasNextPage { get; set; }
    }
}
