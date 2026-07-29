namespace ExpenseTracker.Dtos.Responses
{
    public class ApiResDto<T>
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        //public int? totalCount { get; set; }
        //public int? page { get; set; }
        //public int? limit { get; set; }
        //public bool? hasNextPage { get; set; }
        //public object? errors { get; set; }
        public T? data { get; set; }
    }
}
