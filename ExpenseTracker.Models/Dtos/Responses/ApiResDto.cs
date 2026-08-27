namespace ExpenseTracker.Models.Dtos.Responses
{
    public class ApiResDto<T>
    {
        public bool Success { get; set; }
        public decimal? TotalExpense { get; set; }
        public int? TotalCount { get; set; }
        public HighestRecordResDto? HighestRecord { get; set; }
        public int? Page { get; set; }
        public int? Limit { get; set; }
        public bool? HasNextPage { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Errors { get; set; }
        public T? Data { get; set; }
    }
}
