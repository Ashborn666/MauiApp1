using MySqlConnector;
using System.Diagnostics;

namespace MauiApp1
{
    public static class TestConnection
    {
        public static async Task TestMySqlConnection()
        {
            string connectionString = "Server=192.168.40.10;Port=3307;Database=DB_LOGIN;User=rootlogin;Password=MESSIRONALDO12;SslMode=None;Connection Timeout=10;";

            try
            {
                Debug.WriteLine("🔄 Intentando conectar a MySQL...");

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                Debug.WriteLine("✅ CONEXIÓN EXITOSA!");
                Debug.WriteLine($"📊 Base de datos: {connection.Database}");
                Debug.WriteLine($"🖥️ Servidor: {connection.DataSource}");

                // Probar una consulta simple
                using var command = new MySqlCommand("SELECT COUNT(*) FROM users", connection);
                var count = await command.ExecuteScalarAsync();
                Debug.WriteLine($"👥 Usuarios en la tabla: {count}");

                await connection.CloseAsync();
                Debug.WriteLine("🔒 Conexión cerrada correctamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERROR DE CONEXIÓN:");
                Debug.WriteLine($"   Mensaje: {ex.Message}");
                Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
            }
        }
    }
}