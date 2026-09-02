using ExpenseTracker.Models.Models;

namespace ExpenseTracker.Models.Dtos.Responses
{
    public class RecentUsersResDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
