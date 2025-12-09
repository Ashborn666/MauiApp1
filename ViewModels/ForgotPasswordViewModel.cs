using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1.Services;
using System.Diagnostics;

namespace MauiApp1.ViewModels
{
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string email = "";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool showTokenSent;

        [ObservableProperty]
        private string recoveryToken;

        public ForgotPasswordViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task SendRecoveryCodeAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                ShowTokenSent = false;

                Debug.WriteLine($"Solicitando código de recuperación para: {Email}");

                string token = await _authService.RequestPasswordResetAsync(Email);

                if (!string.IsNullOrEmpty(token))
                {
                    RecoveryToken = token;
                    ShowTokenSent = true;

                    await Application.Current.MainPage.DisplayAlert(
                        "Código Enviado",
                        $"Tu código de recuperación es:\n\n{token}\n\nEste código expira en 1 hora.\n\n(En producción, este código se enviaría por email)",
                        "OK");

                    Debug.WriteLine($"Token generado: {token}");
                }
                else
                {
                    ErrorMessage = "Error al generar el código de recuperación";
                    await Application.Current.MainPage.DisplayAlert("Error", ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en SendRecoveryCodeAsync: {ex.Message}");
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