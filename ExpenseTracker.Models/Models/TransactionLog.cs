namespace ExpenseTracker.Models.Models
{
    public class TransactionLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
        public string? Exception { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Action { get; set; }
        public string? EntityName { get; set; }
        public string? Activity { get; set; }
    }
}
