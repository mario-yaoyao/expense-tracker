using System.ComponentModel.DataAnnotations;

namespace BudgetWise.Models.Dtos.Requests
{
    public class ForgotPasswordReqDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}
