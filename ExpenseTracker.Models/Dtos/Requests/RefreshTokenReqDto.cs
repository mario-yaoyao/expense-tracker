using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dtos.Requests
{
    public class RefreshTokenReqDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MinLength(20)]
        public required string RefreshToken { get; set; }
    }
}
