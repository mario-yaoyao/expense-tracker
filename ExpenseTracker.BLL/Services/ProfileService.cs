using AutoMapper;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Common;
using ExpenseTracker.Models.Dtos.Requests;
using ExpenseTracker.Models.Dtos.Responses;
using ExpenseTracker.Models.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.BLL.Services
{
    public class ProfileService(IProfileRepository profileRepository, IMapper mapper) : IProfileService
    {
        public async Task<ProfileResDto?> GetUserProfileAsync(int userId)
        {
            var user = await profileRepository.GetUserByIdAsync(userId);

            if (user == null) return null;

            return mapper.Map<ProfileResDto>(user);
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordReqDto request)
        {
            var user = await profileRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    ErrorMessage = "User not found."
                };
            }

            if (!IsPasswordValid(user, request.CurrentPassword))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    ErrorMessage = "Current password is incorrect."
                };
            }

            if (IsPasswordValid(user, request.NewPassword))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    ErrorMessage = "New password must be different from your current password."
                };
            }

            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await profileRepository.UpdatePasswordAsync(user);

            return new ServiceResult<bool>
            {
                Success = true,
                Data = true
            };
        }

        private static bool IsPasswordValid(User user, string password) =>
                new PasswordHasher<User>().VerifyHashedPassword(user, user.HashedPassword, password) != PasswordVerificationResult.Failed;
    }
}
