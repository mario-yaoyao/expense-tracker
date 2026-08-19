using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class RegisterReqDto
    {
        [Required]
        [MaxLength(100)]
        [MinLength(2)]
        [RegularExpression(@"^[a-zA-Z\s\.\-]+$",
            ErrorMessage = "Full name contains invalid characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$",
            ErrorMessage = "Username can only contain letters, numbers, underscore.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [MinLength(10)]
        [RegularExpression(@"^(\+63|0)9\d{9}$",
          ErrorMessage = "Invalid Philippine mobile number.")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one letter and one number.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
