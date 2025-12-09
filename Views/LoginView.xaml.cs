using MauiApp1.ViewModels;
using System.Diagnostics;

namespace MauiApp1.Views
{
    public partial class LoginView : ContentPage
    {
        public LoginView(LoginViewModel viewModel)
        {
            try
            {
                InitializeComponent();

                if (viewModel == null)
                {
                    throw new ArgumentNullException(nameof(viewModel), "LoginViewModel no puede ser null");
                }

                BindingContext = viewModel;
                Debug.WriteLine("LoginView inicializado correctamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en LoginView constructor: {ex.Message}");
                throw;
            }
        }
    }
}