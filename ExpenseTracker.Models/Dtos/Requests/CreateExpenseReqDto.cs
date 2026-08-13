using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class CreateExpenseReqDto
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;
    }
}
