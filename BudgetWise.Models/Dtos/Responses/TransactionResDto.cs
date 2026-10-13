using BudgetWise.Models.Models;
using System.Text.Json.Serialization;

namespace BudgetWise.Models.Dtos.Responses
{
    public class TransactionResDto
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string? Username { get; set; } = string.Empty;

        public TransactionType? Type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Activity { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; } = string.Empty;

        public DateTime TimeStamp { get; set; }
    }
}
