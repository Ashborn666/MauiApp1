using MySqlConnector;  // ✅ Cambiado aquí
using MauiApp1.Models;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace MauiApp1.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = "Server=192.168.40.10;Port=3307;Database=DB_LOGIN;User=rootlogin;Password=MESSIRONALDO12;SslMode=None;Connection Timeout=10;";
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                Debug.WriteLine($"Intentando conectar a la base de datos MySQL...");

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                Debug.WriteLine("Conexión exitosa a MySQL");

                string query = @"SELECT id, name, email, password_hash, created_at, is_active 
                                FROM users 
                                WHERE email = @Email AND is_active = 1";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var user = new User
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Email = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4),
                        IsActive = reader.GetBoolean(5)
                    };

                    Debug.WriteLine($"Usuario encontrado: {user.Email}");
                    return user;
                }

                Debug.WriteLine("Usuario no encontrado en la base de datos");
                return null;
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"Error MySQL: {ex.Message}");
                Debug.WriteLine($"Error Number: {ex.Number}");
                throw new Exception($"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general: {ex.Message}");
                throw new Exception($"Error al consultar usuario: {ex.Message}");
            }
        }

        public async Task<bool> CreateUserAsync(string name, string email, string password)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Verificar si el email ya existe
                string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @Email";
                using var checkCommand = new MySqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@Email", email);
                var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                if (count > 0)
                {
                    throw new Exception("El email ya está registrado");
                }

                string query = @"INSERT INTO users (name, email, password_hash, created_at, is_active)
                                VALUES (@Name, @Email, @PasswordHash, @CreatedAt, @IsActive)";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@IsActive", true);

                int result = await command.ExecuteNonQueryAsync();
                Debug.WriteLine($"Usuario creado exitosamente: {email}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al crear usuario: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Eliminar roles anteriores
                string deleteQuery = "DELETE FROM user_roles WHERE user_id = @UserId";
                using var deleteCmd = new MySqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@UserId", userId);
                await deleteCmd.ExecuteNonQueryAsync();

                // Insertar nuevo rol
                string insertQuery = @"INSERT INTO user_roles (user_id, role_id, assigned_at)
                               VALUES (@UserId, 
                                      (SELECT id FROM roles WHERE name = @Role LIMIT 1),
                                      NOW())";

                using var insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@UserId", userId);
                insertCmd.Parameters.AddWithValue("@Role", newRole);

                var result = await insertCmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar rol: {ex.Message}");
                return false;
            }
        }


        public async Task<string> CreatePasswordResetTokenAsync(string email)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Verificar que el usuario existe
                string userQuery = "SELECT id FROM users WHERE email = @Email AND is_active = 1";
                using var userCommand = new MySqlCommand(userQuery, connection);
                userCommand.Parameters.AddWithValue("@Email", email);
                var userId = await userCommand.ExecuteScalarAsync();

                if (userId == null)
                {
                    throw new Exception("Usuario no encontrado");
                }

                // Generar token
                string token = GenerateToken();

                // Guardar token en la base de datos
                string query = @"INSERT INTO password_resets (user_id, token, expires_at, created_at)
                                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt)";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@ExpiresAt", DateTime.Now.AddHours(1));
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                await command.ExecuteNonQueryAsync();

                Debug.WriteLine($"Token de recuperación creado para: {email}");
                return token;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al crear token de recuperación: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyPasswordResetTokenAsync(string token)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"SELECT COUNT(*) FROM password_resets 
                                WHERE token = @Token AND expires_at > @Now";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@Now", DateTime.Now);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al verificar token: {ex.Message}");
                return false;
            }
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateToken()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}