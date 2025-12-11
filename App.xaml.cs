using MauiApp1.Views;

namespace MauiApp1
{
    public partial class App : Application
    {
        public App(LoginView loginView)
        {
            InitializeComponent();

            // La página principal SIEMPRE se obtiene desde DI
            MainPage = new NavigationPage(loginView);
        }
    }
}
