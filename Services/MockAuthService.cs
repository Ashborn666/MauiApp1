using MauiApp1.Models;
using System.Security.Cryptography;
using System.Text;

namespace MauiApp1.Services
{
    public class MockAuthService : IAuthService
    {
        private User _currentUser;

        // Lista persistente en memoria
        private static List<User> _mockUsers = new()
        {
            new User
            {
                Id = 1,
                Name = "Edrian Hiraldo",
                Email = "edrian.hiraldo@utp.ac.pa",
                PasswordHash = HashPassword("12345"),
                IsActive = true,
                Roles = new List<Role> { new Role { Id = 1, Name = "user" } }
            },
            new User
            {
                Id = 2,
                Name = "David Guerra",
                Email = "david.guerra@utp.ac.pa",
                PasswordHash = HashPassword("admin123"),
                IsActive = true,
                Roles = new List<Role> { new Role { Id = 2, Name = "admin" } }
            },
            new User
            {
                Id = 3,
                Name = "Ryan Navarro",
                Email = "ryan.navarro@utp.ac.pa",
                PasswordHash = HashPassword("clave123"),
                IsActive = true,
                Roles = new List<Role> { new Role { Id = 1, Name = "user" } }
            },
        };

        // -----------------------
        // LOGIN
        // -----------------------
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            await Task.Delay(200);

            // SIEMPRE limpiar antes de logear
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_name");
            SecureStorage.Remove("user_role");

            var user = _mockUsers.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return new LoginResponse { IsSuccess = false, Message = "Usuario no encontrado" };

            if (!VerifyPassword(password, user.PasswordHash))
                return new LoginResponse { IsSuccess = false, Message = "Contraseña incorrecta" };

            // Guardar usuario actual
            _currentUser = user;

            // Guardar datos EN ORDEN
            await SecureStorage.SetAsync("user_id", user.Id.ToString());
            await SecureStorage.SetAsync("user_email", user.Email);
            await SecureStorage.SetAsync("user_name", user.Name);
            await SecureStorage.SetAsync("user_role", user.Roles.First().Name);

            return new LoginResponse
            {
                IsSuccess = true,
                Message = "Login exitoso",
                User = user
            };
        }

        public static void ResetSecureStorage()
        {
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_name");
            SecureStorage.Remove("user_role");
        }

        // -----------------------
        // CAMBIO DE ROL
        // -----------------------
        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            var user = _mockUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return false;

            // 🟦 1) Contar cuántos admins hay actualmente
            int adminCount = _mockUsers.Count(u => u.Roles.Any(r => r.Name == "admin"));

            bool userIsAdmin = user.Roles.Any(r => r.Name == "admin");

            // 🟥 2) Si el cambio es ADMIN → USER y este usuario es el ÚLTIMO admin → NO PERMITIR
            if (newRole == "user" && userIsAdmin && adminCount == 1)
            {
                // No se puede dejar el sistema sin admins
                return false;
            }

            // 🟩 3) Aplicar cambio de rol
            user.Roles.Clear();
            user.Roles.Add(new Role
            {
                Id = newRole == "admin" ? 2 : 1,
                Name = newRole
            });

            // 🟦 4) Actualizar también al usuario actual si es el que está logeado
            if (_currentUser != null && _currentUser.Id == userId)
            {
                _currentUser.Roles.Clear();
                _currentUser.Roles.Add(new Role
                {
                    Id = newRole == "admin" ? 2 : 1,
                    Name = newRole
                });

                await SecureStorage.SetAsync("user_role", newRole);
            }

            return true;
        }

        // -----------------------
        // OBTENER USUARIO ACTUAL
        // -----------------------
        public async Task<User> GetCurrentUserAsync()
        {
            var email = await SecureStorage.GetAsync("user_email");
            if (email == null)
                return null;

            _currentUser = _mockUsers.FirstOrDefault(u => u.Email == email);
            return _currentUser;
        }

        // -----------------------
        // OBTENER LISTA DE USUARIOS
        // -----------------------
        public List<User> GetAllMockUsers()
        {
            return _mockUsers;
        }

        // -----------------------
        // REGISTER (obligatorio por la interfaz)
        // -----------------------
        public async Task<LoginResponse> RegisterAsync(string name, string email, string password)
        {
            await Task.Delay(150);

            if (_mockUsers.Any(u => u.Email == email))
                return new LoginResponse { IsSuccess = false, Message = "El usuario ya existe" };

            var newUser = new User
            {
                Id = _mockUsers.Max(u => u.Id) + 1,
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password),
                IsActive = true,
                Roles = new List<Role> { new Role { Id = 1, Name = "user" } }
            };

            _mockUsers.Add(newUser);

            return new LoginResponse
            {
                IsSuccess = true,
                Message = "Usuario registrado correctamente",
                User = newUser
            };
        }

        // -----------------------
        // OLVIDÉ MI CONTRASEÑA
        // -----------------------
        public async Task<string> RequestPasswordResetAsync(string email)
        {
            await Task.Delay(150);

            var user = _mockUsers.FirstOrDefault(u => u.Email == email);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            return "MOCK-TOKEN-1234";
        }

        // -----------------------
        // LOGOUT
        // -----------------------
        public async Task<bool> LogoutAsync()
        {
            _currentUser = null;

            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_name");
            SecureStorage.Remove("user_role");

            return await Task.FromResult(true);
        }

        // -----------------------
        // AUTENTICADO
        // -----------------------
        public async Task<bool> IsAuthenticatedAsync()
        {
            var id = await SecureStorage.GetAsync("user_id");
            return id != null;
        }

        // -----------------------
        // HELPERS
        // -----------------------
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
            );
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
