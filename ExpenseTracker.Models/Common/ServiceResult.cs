namespace ExpenseTracker.Models.Common
{
    public class ServiceResult<T>
    {
        public bool success { get; set; }

        public string message { get; set; } = string.Empty;

        public T? data { get; set; }
    }
}
