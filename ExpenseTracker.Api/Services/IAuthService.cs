using ExpenseTracker.Dtos.Requests;
using ExpenseTracker.Dtos.Responses;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public interface IAuthService
    {
        Task<(User? User, string? Error)> RegisterAsync(RegisterReqDto request);
        Task<(TokenResDto? Token, string? Error)> LoginAsync(LoginUserReqDto request);
        Task<TokenResDto?> RefreshTokensAsync(RefreshTokenReqDto request);
    }
}
