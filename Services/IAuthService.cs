using MauiApp1.Models;

namespace MauiApp1.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<LoginResponse> RegisterAsync(string name, string email, string password);
        Task<string> RequestPasswordResetAsync(string email);
        Task<bool> LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<User> GetCurrentUserAsync();
    }
}