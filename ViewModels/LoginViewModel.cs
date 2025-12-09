using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using MauiApp1.Views;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MauiApp1.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string email = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Debug.WriteLine($"Intentando login con email: {Email}");

                var response = await _authService.LoginAsync(Email, Password);

                Debug.WriteLine($"Respuesta del login: {response.IsSuccess} - {response.Message}");

                if (response.IsSuccess)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Application.Current.MainPage = new AppShell();
                    });
                }
                else
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en LoginAsync: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                ErrorMessage = $"Error: {ex.Message}";

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Ocurrió un error al intentar iniciar sesión:\n{ex.Message}",
                        "OK");
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreateAccountAsync()
        {
            await Shell.Current.GoToAsync(nameof(RegisterView));
        }

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            await Shell.Current.GoToAsync(nameof(ForgotPasswordView));
        }
    }
}