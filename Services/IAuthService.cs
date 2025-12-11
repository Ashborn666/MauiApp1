using MauiApp1.Models;
using System.Collections.Generic;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(string email, string password);
    Task<LoginResponse> RegisterAsync(string name, string email, string password);
    Task<string> RequestPasswordResetAsync(string email);
    Task<bool> LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<User> GetCurrentUserAsync();

    // Cambiar roles
    Task<bool> UpdateUserRoleAsync(int userId, string newRole);

    // ⭐ NUEVO: requerido por UsersListView
    List<User> GetAllMockUsers();
}
