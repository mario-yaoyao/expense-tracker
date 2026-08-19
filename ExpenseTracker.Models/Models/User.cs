namespace ExpenseTracker.Models.Models
{
    public enum UserRole
    {
        SuperAdmin,
        User
    }
    public class User : Base
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsActive { get; set; } = false;

        public ICollection<Expense> Expenses { get; set; } = [];
    }
}
