using System.Text.Json.Serialization;

namespace BudgetWise.Models.Dtos.Responses
{
    public class RecentTransactionsResDto
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string? Username { get; set; } = string.Empty;

        public string? Action { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Activity { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
