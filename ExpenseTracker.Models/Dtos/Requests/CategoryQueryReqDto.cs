using ExpenseTracker.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class CategoryQueryReqDto
    {
        public CategoryType? Type { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int Limit { get; set; } = 20;

        public string? Search { get; set; }
    }
}
