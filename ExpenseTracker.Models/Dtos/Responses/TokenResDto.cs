namespace ExpenseTracker.Models.Dtos.Responses
{
    public class TokenResDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
