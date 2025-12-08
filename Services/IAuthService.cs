using MauiApp1.Models;

namespace MauiApp1.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<bool> LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<User> GetCurrentUserAsync();
    }
}