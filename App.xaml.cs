using MauiApp1.Views;
using MauiApp1.ViewModels;
using MauiApp1.Services;
using System.Diagnostics;

namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            try
            {
                // USAR MOCK SERVICE PARA PRUEBAS (sin base de datos)
                var authService = new MockAuthService();

                // Para usar la base de datos real, descomentar estas líneas:
                // var databaseService = new DatabaseService();
                // var authService = new AuthService(databaseService);

                var loginViewModel = new LoginViewModel(authService);
                var loginView = new LoginView(loginViewModel);
                MainPage = loginView;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al inicializar la app: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                // Mostrar una página de error
                MainPage = new ContentPage
                {
                    Content = new VerticalStackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                Text = "Error al iniciar la aplicación",
                                FontSize = 20,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = ex.Message,
                                FontSize = 14,
                                HorizontalOptions = LayoutOptions.Center,
                                Padding = 20
                            }
                        },
                        VerticalOptions = LayoutOptions.Center,
                        Padding = 30
                    }
                };
            }
        }
    }
}