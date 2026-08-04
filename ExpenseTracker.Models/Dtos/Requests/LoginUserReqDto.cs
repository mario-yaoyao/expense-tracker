using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class LoginUserReqDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
