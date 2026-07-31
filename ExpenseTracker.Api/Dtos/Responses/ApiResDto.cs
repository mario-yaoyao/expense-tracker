namespace ExpenseTracker.Dtos.Responses
{
    public class ApiResDto<T>
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public T? data { get; set; }
    }
}
