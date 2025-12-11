using MauiApp1.Views;
using MauiApp1.ViewModels;
using MauiApp1.Services;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("IsLogged");

            var authService = new MockAuthService();
            var vm = new LoginViewModel(authService);
            var view = new LoginView(vm);

            Application.Current.MainPage = new NavigationPage(view);
        }
    }
}
