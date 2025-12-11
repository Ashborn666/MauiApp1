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

                // Registrar usuario
                var response = await _authService.RegisterAsync(Name, Email, Password);

                if (!response.IsSuccess)
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert("Error", response.Message, "OK");
                    return;
                }

                // Login automático después de registrar
                var loginResponse = await _authService.LoginAsync(Email, Password);

                if (loginResponse.IsSuccess)
                {
                    // Ir directamente al AppShell como en login normal
                    Application.Current.MainPage = new AppShell();
                    Shell.Current.GoToAsync("//HomePage");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", loginResponse.Message, "OK");

                    // Regresar al login si algo falla
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
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
