using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using System.Diagnostics;

namespace MauiApp1.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string email = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task CreateAccountAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Debug.WriteLine($"Intentando crear cuenta con: {Name} - {Email}");

                var response = await _authService.RegisterAsync(Name, Email, Password);

                Debug.WriteLine($"Respuesta del registro: {response.IsSuccess} - {response.Message}");

                if (response.IsSuccess)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Cuenta creada exitosamente. Ahora puedes iniciar sesión.", "OK");

                    // Intentar login automático
                    var loginResponse = await _authService.LoginAsync(Email, Password);

                    if (loginResponse.IsSuccess)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Application.Current.MainPage = new AppShell();
                        });
                    }
                    else
                    {
                        // Regresar al login
                        await Application.Current.MainPage.Navigation.PopAsync();
                    }
                }
                else
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CreateAccountAsync: {ex.Message}");
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task BackToLoginAsync()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}