using System.ComponentModel.DataAnnotations;

namespace BudgetWise.Models.Dtos.Requests
{
    public class RefreshTokenReqDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MinLength(20)]
        public required string RefreshToken { get; set; }
    }
}
