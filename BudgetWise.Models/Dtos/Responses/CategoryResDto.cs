using BudgetWise.Models.Models;
using System.Text.Json.Serialization;

namespace BudgetWise.Models.Dtos.Responses
{
    public class CategoryResDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Username { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public CategoryType Type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal? BudgetAmount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal TotalSpent { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public decimal? Remaining { get; set; }
        
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
