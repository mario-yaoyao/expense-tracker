using AutoMapper;
using BudgetWise.BLL.Interfaces;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Serilog;

namespace BudgetWise.BLL.Services
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

            var type = existingUser.IsActive
                ? TransactionType.Create
                : TransactionType.Delete;

            var activity = existingUser.IsActive
                ? "Account activated."
                : "Account deactivated.";

            var message = existingUser.IsActive
                ? $"'{username}' activated user '{existingUser.Username}'."
                : $"'{username}' deactivated user '{existingUser.Username}'.";

            Log.ForContext("IsAuditLog", true)
               .ForContext("UserId", userId)
               .ForContext("Username", existingUser!.Username)
               .ForContext("Type", type)
               .ForContext("EntityName", "User")
               .ForContext("Activity", activity)
               .Information(message);

            return true;
        }
    }
}

