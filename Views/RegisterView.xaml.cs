using MauiApp1.ViewModels;
using System.Diagnostics;

namespace MauiApp1.Views
{
    public partial class RegisterView : ContentPage
    {
        public RegisterView(RegisterViewModel viewModel)
        {
            try
            {
                InitializeComponent();

                if (viewModel == null)
                {
                    throw new ArgumentNullException(nameof(viewModel), "RegisterViewModel no puede ser null");
                }

                BindingContext = viewModel;
                Debug.WriteLine("RegisterView inicializado correctamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en RegisterView constructor: {ex.Message}");
                throw;
            }
        }
    }
}