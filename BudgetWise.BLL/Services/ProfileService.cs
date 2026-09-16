using AutoMapper;
using BudgetWise.BLL.Interfaces;
using BudgetWise.DAL.Interfaces;
using BudgetWise.Models.Common;
using BudgetWise.Models.Dtos.Requests;
using BudgetWise.Models.Dtos.Responses;
using BudgetWise.Models.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace BudgetWise.BLL.Services
{
    public class ProfileService(IProfileRepository profileRepository, IUserRepository userRepository, IMapper mapper) : IProfileService
    {
        public async Task<UserResDto?> GetUserProfileAsync(int userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);

            if (user == null) return null;

            return mapper.Map<UserResDto>(user);
        }

        public async Task<ServiceResult<object>> ChangePasswordAsync(int userId, ChangePasswordReqDto request)
        {
            var user = await userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "User not found."
                };
            }

            if (!IsPasswordValid(user, request.CurrentPassword))
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "Current password is incorrect."
                };
            }

            if (IsPasswordValid(user, request.NewPassword))
            {
                return new ServiceResult<object>
                {
                    Success = false,
                    ErrorMessage = "New password must be different from your current password."
                };
            }

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await profileRepository.UpdatePasswordAsync(user);

            Log.ForContext("UserId", userId)
               .ForContext("Username", user.Username)
               .ForContext("Action", "Update")
               .ForContext("EntityName", "Profile")
               .ForContext("Activity", $"Password changed.'.")
               .Information($"'{user.Username}' password changed.");

            return new ServiceResult<object>
            {
                Success = true
            };
        }

        private static bool IsPasswordValid(User user, string password) =>
                new PasswordHasher<User>().VerifyHashedPassword(user, user.HashedPassword, password) != PasswordVerificationResult.Failed;
    }
}
