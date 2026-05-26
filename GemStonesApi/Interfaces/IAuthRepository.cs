using GemStonesApi.Models;

namespace GemStonesApi.Interfaces
{
    public interface IAuthRepository
    {
        Task<int> RegisterUserAsync(User user);
        Task<User> GetUserByUsernameAsync(string username);

        Task UpdateLoginAttemptsAsync(
           string username,
           int failedAttempts,
           DateTime? lockoutUntil);
        Task ResetLoginAttemptsAsync(string username);
    }
}