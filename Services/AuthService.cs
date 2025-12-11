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
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Email y contraseña son requeridos"
                    };
                }

                var user = await _databaseService.GetUserByEmailAsync(email);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Usuario no encontrado"
                    };
                }

                if (!_databaseService.VerifyPassword(password, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Contraseña incorrecta"
                    };
                }

                _currentUser = user;

                // Guardar datos en SecureStorage
                await SecureStorage.SetAsync("user_id", user.Id.ToString());
                await SecureStorage.SetAsync("user_email", user.Email);
                await SecureStorage.SetAsync("user_name", user.Name);

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

        // ✨ NUEVO — Implementación requerida por IAuthService
        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newRole))
                    throw new Exception("El rol no puede estar vacío");

                // Por ahora no usamos BD real para roles
                // Solo devolvemos éxito automático
                return await Task.FromResult(true);
            }
            catch
            {
                return false;
            }
        }

        public async Task<LoginResponse> RegisterAsync(string name, string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Todos los campos son requeridos"
                    };
                }

                if (password.Length < 5)
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "La contraseña debe tener al menos 5 caracteres"
                    };
                }

                bool success = await _databaseService.CreateUserAsync(name, email, password);

                if (success)
                {
                    return new LoginResponse
                    {
                        IsSuccess = true,
                        Message = "Cuenta creada exitosamente"
                    };
                }

                return new LoginResponse
                {
                    IsSuccess = false,
                    Message = "Error al crear la cuenta"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<string> RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("El email es requerido");

            return await _databaseService.CreatePasswordResetTokenAsync(email);
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _currentUser = null;

                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_email");
                SecureStorage.Remove("user_name");

                return true;
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

        public List<User> GetAllMockUsers()
        {
            return new List<User>(); // Vacía porque AuthService real todavía no usa DB
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
