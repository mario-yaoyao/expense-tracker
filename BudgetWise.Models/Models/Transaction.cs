namespace BudgetWise.Models.Models
{
    public enum TransactionType
    {
        Create,
        Update,
        Delete,
        Info
    }

    public class Transaction
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
        public string? Exception { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public TransactionType? Type { get; set; }
        public string? EntityName { get; set; }
        public string? Activity { get; set; }

        public User? User { get; set; }
    }
}
