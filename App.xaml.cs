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

            // PRUEBA DE CONEXIÓN - Comentar después de probar
            Task.Run(async () => await TestConnection.TestMySqlConnection());
            try
            {
                // Obtener servicios del contenedor DI
                var authService = Handler.MauiContext.Services.GetService<IAuthService>();
                var loginViewModel = Handler.MauiContext.Services.GetService<LoginViewModel>();
                var loginView = Handler.MauiContext.Services.GetService<LoginView>();

                MainPage = new NavigationPage(loginView);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al inicializar la app: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");

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