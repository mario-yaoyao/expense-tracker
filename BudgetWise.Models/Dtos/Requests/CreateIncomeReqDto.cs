using System.ComponentModel.DataAnnotations;

namespace BudgetWise.Models.Dtos.Requests
{
    public class CreateIncomeReqDto
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
