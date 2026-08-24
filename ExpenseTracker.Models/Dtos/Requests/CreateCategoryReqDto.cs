using ExpenseTracker.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class CreateCategoryReqDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public CategoryType Type { get; set; }
    }
}
