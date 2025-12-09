using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class ForgotPasswordView : ContentPage
    {
        public ForgotPasswordView(ForgotPasswordViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}