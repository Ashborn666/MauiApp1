using MauiApp1.ViewModels;
using System.Diagnostics;

namespace MauiApp1.Views
{
    public partial class ForgotPasswordView : ContentPage
    {
        public ForgotPasswordView(ForgotPasswordViewModel viewModel)
        {
            try
            {
                InitializeComponent();

                if (viewModel == null)
                {
                    throw new ArgumentNullException(nameof(viewModel), "ForgotPasswordViewModel no puede ser null");
                }

                BindingContext = viewModel;
                Debug.WriteLine("ForgotPasswordView inicializado correctamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en ForgotPasswordView constructor: {ex.Message}");
                throw;
            }
        }
    }
}