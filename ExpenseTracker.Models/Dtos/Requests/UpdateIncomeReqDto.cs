
namespace ExpenseTracker.Models.Dtos.Requests
{
    public class UpdateIncomeReqDto
    {
        public string Description { get; set; } = string.Empty;

        public decimal? Amount { get; set; }

        public int? CategoryId { get; set; }
    }
}
