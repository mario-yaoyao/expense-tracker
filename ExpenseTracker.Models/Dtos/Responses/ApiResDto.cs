namespace ExpenseTracker.Models.Dtos.Responses
{
    public class ApiResDto<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
        public object? Errors { get; set; }
    }
}
