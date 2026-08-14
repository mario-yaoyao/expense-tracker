using AutoMapper;
using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.Models.Dtos.Responses;

namespace ExpenseTracker.BLL.Services
{
    public class UserService(IUserRepository userRepository, IMapper mapper) : IUserService
    {
        public async Task<UserResDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await userRepository.GetUserProfileAsync(userId);

            if (user == null) return null;

            return mapper.Map<UserResDto>(user);
        }
    }
}
