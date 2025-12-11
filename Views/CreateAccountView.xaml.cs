using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class CreateAccountView : ContentPage
    {
        public CreateAccountView(RegisterViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private async void OnCreateAccountClicked(object sender, EventArgs e)
        {
            if (BindingContext is RegisterViewModel vm)
                await vm.CreateAccountCommand.ExecuteAsync(null);
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
