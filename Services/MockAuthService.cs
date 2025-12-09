using MauiApp1.Models;
using System.Security.Cryptography;
using System.Text;

namespace MauiApp1.Services
{
    public class MockAuthService : IAuthService
    {
        private User _currentUser;

        // Lista de usuarios simulados (como si vinieran de la BD)
        private static List<User> _mockUsers = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "edrian.hiraldo@utp.ac.pa",
                FullName = "Edrian Hiraldo",
                PasswordHash = HashPassword("12345"),
                CreatedAt = DateTime.Now.AddMonths(-6),
                IsActive = true
            },
            new User
            {
                Id = 2,
                Email = "admin@utp.ac.pa",
                FullName = "Administrador UTP",
                PasswordHash = HashPassword("admin123"),
                CreatedAt = DateTime.Now.AddYears(-1),
                IsActive = true
            },
            new User
            {
                Id = 3,
                Email = "maria.gonzalez@utp.ac.pa",
                FullName = "María González",
                PasswordHash = HashPassword("pass123"),
                CreatedAt = DateTime.Now.AddMonths(-3),
                IsActive = true
            },
            new User
            {
                Id = 4,
                Email = "juan.perez@utp.ac.pa",
                FullName = "Juan Pérez",
                PasswordHash = HashPassword("qwerty"),
                CreatedAt = DateTime.Now.AddMonths(-2),
                IsActive = true
            },
            new User
            {
                Id = 5,
                Email = "test@test.com",
                FullName = "Usuario de Prueba",
                PasswordHash = HashPassword("test"),
                CreatedAt = DateTime.Now.AddDays(-10),
                IsActive = true
            }
        };

        // Diccionario para tokens de recuperación
        private static Dictionary<string, (string Email, DateTime ExpiresAt)> _resetTokens = new Dictionary<string, (string, DateTime)>();

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            await Task.Delay(800); // Simular delay de red

            try
            {
                // Buscar usuario por email
                var user = _mockUsers.FirstOrDefault(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                    u.IsActive);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Usuario no encontrado"
                    };
                }

                // Verificar contraseña
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "Contraseña incorrecta"
                    };
                }

                // Login exitoso
                _currentUser = user;

                // Guardar en SecureStorage
                await SecureStorage.SetAsync("user_id", user.Id.ToString());
                await SecureStorage.SetAsync("user_email", user.Email);
                await SecureStorage.SetAsync("user_name", user.FullName);

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
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponse> RegisterAsync(string name, string email, string password)
        {
            await Task.Delay(800); // Simular delay de red

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
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

                // Verificar si el email ya existe
                if (_mockUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = "El email ya está registrado"
                    };
                }

                // Crear nuevo usuario
                var newUser = new User
                {
                    Id = _mockUsers.Count > 0 ? _mockUsers.Max(u => u.Id) + 1 : 1,
                    Email = email,
                    FullName = name,
                    PasswordHash = HashPassword(password),
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                // Agregar a la lista
                _mockUsers.Add(newUser);

                return new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Usuario registrado exitosamente",
                    User = newUser
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    IsSuccess = false,
                    Message = $"Error al registrar: {ex.Message}"
                };
            }
        }

        public async Task<string> RequestPasswordResetAsync(string email)
        {
            await Task.Delay(500); // Simular delay de red

            try
            {
                // Verificar que el usuario existe
                var user = _mockUsers.FirstOrDefault(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                    u.IsActive);

                if (user == null)
                {
                    throw new Exception("Usuario no encontrado");
                }

                // Generar token único
                string token = GenerateResetToken();

                // Guardar token con fecha de expiración (1 hora)
                _resetTokens[token] = (email, DateTime.Now.AddHours(1));

                // Limpiar tokens expirados
                CleanExpiredTokens();

                return token;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al solicitar recuperación: {ex.Message}");
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _currentUser = null;
                SecureStorage.Remove("user_id");
                SecureStorage.Remove("user_email");
                SecureStorage.Remove("user_name");
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
                // Buscar usuario por email en la lista
                _currentUser = _mockUsers.FirstOrDefault(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            }

            return _currentUser;
        }

        // Método público para obtener todos los usuarios (útil para debugging)
        public List<User> GetAllMockUsers()
        {
            return _mockUsers.Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive
                // NO retornamos el PasswordHash por seguridad
            }).ToList();
        }

        // Método para verificar si un token de reset es válido
        public bool VerifyResetToken(string token)
        {
            if (_resetTokens.TryGetValue(token, out var tokenInfo))
            {
                return tokenInfo.ExpiresAt > DateTime.Now;
            }
            return false;
        }

        // Método para resetear contraseña con token
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            await Task.Delay(500);

            if (!_resetTokens.TryGetValue(token, out var tokenInfo))
            {
                return false;
            }

            if (tokenInfo.ExpiresAt <= DateTime.Now)
            {
                _resetTokens.Remove(token);
                return false;
            }

            var user = _mockUsers.FirstOrDefault(u =>
                u.Email.Equals(tokenInfo.Email, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                user.PasswordHash = HashPassword(newPassword);
                _resetTokens.Remove(token);
                return true;
            }

            return false;
        }

        #region Métodos auxiliares privados

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }

        private string GenerateResetToken()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 32); // Token de 32 caracteres
        }

        private void CleanExpiredTokens()
        {
            var expiredTokens = _resetTokens
                .Where(t => t.Value.ExpiresAt <= DateTime.Now)
                .Select(t => t.Key)
                .ToList();

            foreach (var token in expiredTokens)
            {
                _resetTokens.Remove(token);
            }
        }

        #endregion
    }
}