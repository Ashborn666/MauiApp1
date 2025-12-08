using Microsoft.Data.SqlClient;
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
            // IMPORTANTE: Actualiza esta cadena de conexión con tus datos reales
            _connectionString = "Server=TU_IP_AQUI;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;Connection Timeout=5;";
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                Debug.WriteLine($"Intentando conectar a la base de datos...");

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                Debug.WriteLine("Conexión exitosa a la base de datos");

                string query = @"SELECT Id, Email, PasswordHash, FullName, CreatedAt, IsActive 
                                FROM Users 
                                WHERE Email = @Email AND IsActive = 1";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var user = new User
                    {
                        Id = reader.GetInt32(0),
                        Email = reader.GetString(1),
                        PasswordHash = reader.GetString(2),
                        FullName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4),
                        IsActive = reader.GetBoolean(5)
                    };

                    Debug.WriteLine($"Usuario encontrado: {user.Email}");
                    return user;
                }

                Debug.WriteLine("Usuario no encontrado en la base de datos");
                return null;
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"Error SQL: {ex.Message}");
                Debug.WriteLine($"Error Number: {ex.Number}");
                throw new Exception($"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general: {ex.Message}");
                throw new Exception($"Error al consultar usuario: {ex.Message}");
            }
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                string query = @"INSERT INTO Users (Email, PasswordHash, FullName, CreatedAt, IsActive)
                                VALUES (@Email, @PasswordHash, @FullName, @CreatedAt, @IsActive)";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                command.Parameters.AddWithValue("@FullName", user.FullName ?? "");
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@IsActive", true);

                int result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al crear usuario: {ex.Message}");
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
    }
}