using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class AuthService : IAuthService
    {
        private readonly DatabaseService _databaseService;
        private User _currentUser;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Email y contraseña son requeridos"
                    };
                }

                // Buscar usuario en la base de datos
                var user = await _databaseService.GetUserByEmailAsync(email);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Usuario no encontrado"
                    };
                }

                // Verificar contraseña
                if (!_databaseService.VerifyPassword(password, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Contraseña incorrecta"
                    };
                }

                // Login exitoso
                _currentUser = user;

                // Guardar datos de sesión
                await SecureStorage.SetAsync("user_id", user.Id.ToString());
                await SecureStorage.SetAsync("user_email", user.Email);

                return new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Login exitoso",
                    User = user
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    IsSuccess = false,
                    Message = $"Error al iniciar sesión: {ex.Message}"
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _currentUser = null;
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_email");
                return await Task.FromResult(true);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var userId = await SecureStorage.GetAsync("user_id");
            return !string.IsNullOrEmpty(userId);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            if (_currentUser != null)
                return _currentUser;

            var email = await SecureStorage.GetAsync("user_email");
            if (!string.IsNullOrEmpty(email))
            {
                _currentUser = await _databaseService.GetUserByEmailAsync(email);
            }

            return _currentUser;
        }
    }
}