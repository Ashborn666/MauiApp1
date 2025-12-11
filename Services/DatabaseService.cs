using MySqlConnector;
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
            // ⚙️ ACTUALIZA AQUÍ CON TU CONFIGURACIÓN
            _connectionString = "Server=DellInspiron;Port=3306;Database=DB_LOGIN;User=root;Password=277353;SslMode=None;Connection Timeout=10;";
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                Debug.WriteLine($"🔄 Conectando a MySQL...");

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                Debug.WriteLine("✅ Conexión exitosa a MySQL");

                // Consulta con JOIN para obtener roles
                string query = @"
                    SELECT 
                        u.id, u.name, u.email, u.password_hash, 
                        u.created_at, u.is_active,
                        r.id as role_id, r.name as role_name
                    FROM users u
                    LEFT JOIN user_roles ur ON u.id = ur.user_id
                    LEFT JOIN roles r ON ur.role_id = r.id
                    WHERE u.email = @Email AND u.is_active = 1";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                using var reader = await command.ExecuteReaderAsync();

                User user = null;

                while (await reader.ReadAsync())
                {
                    // Primera iteración: crear usuario
                    if (user == null)
                    {
                        user = new User
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.IsDBNull(reader.GetOrdinal("name")) ? "" : reader.GetString("name"),
                            Email = reader.GetString("email"),
                            PasswordHash = reader.GetString("password_hash"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            IsActive = reader.GetBoolean("is_active"),
                            Roles = new List<Role>()
                        };
                    }

                    // Agregar rol si existe
                    if (!reader.IsDBNull(reader.GetOrdinal("role_id")))
                    {
                        user.Roles.Add(new Role
                        {
                            Id = reader.GetInt32("role_id"),
                            Name = reader.GetString("role_name")
                        });
                    }
                }

                // Si no tiene roles, asignar "user" por defecto
                if (user != null && user.Roles.Count == 0)
                {
                    user.Roles.Add(new Role { Id = 1, Name = "user" });
                }

                Debug.WriteLine(user != null
                    ? $"✅ Usuario encontrado: {user.Email} - Rol: {user.Roles.FirstOrDefault()?.Name}"
                    : "❌ Usuario no encontrado");

                return user;
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"❌ Error MySQL: {ex.Message}");
                throw new Exception($"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error general: {ex.Message}");
                throw new Exception($"Error al consultar usuario: {ex.Message}");
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        u.id, u.name, u.email, u.password_hash,
                        u.created_at, u.is_active,
                        r.id as role_id, r.name as role_name
                    FROM users u
                    LEFT JOIN user_roles ur ON u.id = ur.user_id
                    LEFT JOIN roles r ON ur.role_id = r.id
                    WHERE u.is_active = 1
                    ORDER BY u.id";

                using var command = new MySqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var usersDict = new Dictionary<int, User>();

                while (await reader.ReadAsync())
                {
                    int userId = reader.GetInt32("id");

                    // Si el usuario no existe en el diccionario, agregarlo
                    if (!usersDict.ContainsKey(userId))
                    {
                        usersDict[userId] = new User
                        {
                            Id = userId,
                            Name = reader.IsDBNull(reader.GetOrdinal("name")) ? "" : reader.GetString("name"),
                            Email = reader.GetString("email"),
                            PasswordHash = reader.GetString("password_hash"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            IsActive = reader.GetBoolean("is_active"),
                            Roles = new List<Role>()
                        };
                    }

                    // Agregar rol si existe
                    if (!reader.IsDBNull(reader.GetOrdinal("role_id")))
                    {
                        usersDict[userId].Roles.Add(new Role
                        {
                            Id = reader.GetInt32("role_id"),
                            Name = reader.GetString("role_name")
                        });
                    }
                }

                // Asignar rol "user" por defecto a usuarios sin rol
                foreach (var user in usersDict.Values.Where(u => u.Roles.Count == 0))
                {
                    user.Roles.Add(new Role { Id = 1, Name = "user" });
                }

                return usersDict.Values.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al obtener usuarios: {ex.Message}");
                throw;
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

                // Insertar usuario
                string insertUserQuery = @"
                    INSERT INTO users (name, email, password_hash, created_at, is_active)
                    VALUES (@Name, @Email, @PasswordHash, @CreatedAt, @IsActive);
                    SELECT LAST_INSERT_ID();";

                using var insertCommand = new MySqlCommand(insertUserQuery, connection);
                insertCommand.Parameters.AddWithValue("@Name", name);
                insertCommand.Parameters.AddWithValue("@Email", email);
                insertCommand.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                insertCommand.Parameters.AddWithValue("@IsActive", true);

                var userId = Convert.ToInt32(await insertCommand.ExecuteScalarAsync());

                // Asignar rol "user" por defecto
                string insertRoleQuery = @"
                    INSERT INTO user_roles (user_id, role_id)
                    VALUES (@UserId, (SELECT id FROM roles WHERE name = 'user' LIMIT 1))";

                using var roleCommand = new MySqlCommand(insertRoleQuery, connection);
                roleCommand.Parameters.AddWithValue("@UserId", userId);
                await roleCommand.ExecuteNonQueryAsync();

                Debug.WriteLine($"✅ Usuario creado: {email} con rol 'user'");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al crear usuario: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Verificar cuántos admins hay actualmente
                string countAdminsQuery = @"
                    SELECT COUNT(DISTINCT ur.user_id) 
                    FROM user_roles ur
                    JOIN roles r ON ur.role_id = r.id
                    WHERE r.name = 'admin'";

                using var countCommand = new MySqlCommand(countAdminsQuery, connection);
                int adminCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

                // Verificar si el usuario actual es admin
                string checkUserRoleQuery = @"
                    SELECT r.name 
                    FROM user_roles ur
                    JOIN roles r ON ur.role_id = r.id
                    WHERE ur.user_id = @UserId";

                using var checkCommand = new MySqlCommand(checkUserRoleQuery, connection);
                checkCommand.Parameters.AddWithValue("@UserId", userId);
                var currentRole = await checkCommand.ExecuteScalarAsync() as string;

                // No permitir cambiar el último admin a user
                if (currentRole == "admin" && newRole == "user" && adminCount == 1)
                {
                    Debug.WriteLine("❌ No se puede cambiar el último admin a user");
                    return false;
                }

                // Eliminar roles anteriores
                string deleteQuery = "DELETE FROM user_roles WHERE user_id = @UserId";
                using var deleteCmd = new MySqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@UserId", userId);
                await deleteCmd.ExecuteNonQueryAsync();

                // Insertar nuevo rol
                string insertQuery = @"
                    INSERT INTO user_roles (user_id, role_id, assigned_at)
                    VALUES (@UserId, (SELECT id FROM roles WHERE name = @Role LIMIT 1), NOW())";

                using var insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@UserId", userId);
                insertCmd.Parameters.AddWithValue("@Role", newRole);

                var result = await insertCmd.ExecuteNonQueryAsync();

                Debug.WriteLine($"✅ Rol actualizado para usuario {userId}: {newRole}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al actualizar rol: {ex.Message}");
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

                // Guardar token
                string query = @"
                    INSERT INTO password_resets (user_id, token, expires_at, created_at)
                    VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt)";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Token", token);
                command.Parameters.AddWithValue("@ExpiresAt", DateTime.Now.AddHours(1));
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                await command.ExecuteNonQueryAsync();

                Debug.WriteLine($"✅ Token de recuperación creado para: {email}");
                return token;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al crear token: {ex.Message}");
                throw;
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
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}