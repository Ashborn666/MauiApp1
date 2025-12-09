using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class RegisterView : ContentPage
    {
        public RegisterView(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}