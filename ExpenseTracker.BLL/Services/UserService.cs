using AutoMapper;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using Serilog;

namespace ExpenseTracker.BLL.Services
{
    public class UserService(IUserRepository userRepository, IMapper mapper) : IUserService
    {
        public async Task<(List<UserResDto> data, int totalCount, bool hasNextPage)> GetUsersAsync(UserQueryReqDto request)
        {
            var (data, totalCount, hasNextPage) = await userRepository.GetUsersAsync(request.Page, request.Limit, request.Search, request.StartDate, request.EndDate);

            return (
                mapper.Map<List<UserResDto>>(data),
                totalCount,
                hasNextPage
            );
        }

        public async Task<UserResDto?> GetUserByIdAsync(int userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);

            return mapper.Map<UserResDto>(user);
        }

        public async Task<bool> ToggleUserStatusAsync(string username, int userId)
        {
            var existingUser = await userRepository.GetUserByIdAsync(userId);

            if (existingUser == null) return false;

            existingUser.IsActive = !existingUser.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await userRepository.SaveChangesAsync();

            var action = existingUser.IsActive
                ? "Create"
                : "Delete";

            var activity = existingUser.IsActive
                ? "Account activated."
                : "Account deactivated.";

            var message = existingUser.IsActive
                ? $"'{username}' activated user '{existingUser.Username}'."
                : $"'{username}' deactivated user '{existingUser.Username}'.";

            Log.ForContext("UserId", userId)
               .ForContext("Username", existingUser!.Username)
               .ForContext("Action", action)
               .ForContext("EntityName", "User")
               .ForContext("Activity", activity)
               .Information(message);

            return true;
        }
    }
}

