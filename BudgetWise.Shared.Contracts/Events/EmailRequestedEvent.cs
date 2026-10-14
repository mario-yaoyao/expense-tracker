namespace BudgetWise.Shared.Contracts.Events
{
    public class EmailRequestedEvent
    {
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string ResetToken { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}
