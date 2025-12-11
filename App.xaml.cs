using MauiApp1.Views;

namespace MauiApp1
{
    public partial class App : Application
    {
        public App(LoginView loginView)
        {
            InitializeComponent();

            // 🔥 PRUEBA DE CONEXIÓN (temporal)
            Task.Run(async () => await TestConnection.TestMySqlConnection());

            MainPage = new NavigationPage(loginView);
        }
    }
}