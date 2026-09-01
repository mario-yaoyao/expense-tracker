using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class ExpenseQueryReqDto
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int Limit { get; set; } = 20;

        public string? Search { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}
