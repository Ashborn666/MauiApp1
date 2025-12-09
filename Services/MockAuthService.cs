using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class MockAuthService : IAuthService
    {
        private User _currentUser;

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            await Task.Delay(1000); // Simular delay de red

            // Credenciales de prueba
            if (email == "edrian.hiraldo@utp.ac.pa" && password == "12345")
            {
                _currentUser = new User
                {
                    Id = 1,
                    Email = email,
                    FullName = "Edrian Hiraldo",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                await SecureStorage.SetAsync("user_id", _currentUser.Id.ToString());
                await SecureStorage.SetAsync("user_email", _currentUser.Email);

                return new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Login exitoso",
                    User = _currentUser
                };
            }

            return new LoginResponse
            {
                IsSuccess = false,
                Message = "Credenciales incorrectas"
            };
        }

        public async Task<bool> LogoutAsync()
        {
            _currentUser = null;
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_email");
            return await Task.FromResult(true);
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
                _currentUser = new User
                {
                    Id = 1,
                    Email = email,
                    FullName = "Usuario de Prueba",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
            }

            return _currentUser;
        }

        public Task<LoginResponse> RegisterAsync(string name, string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<string> RequestPasswordResetAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}