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
                Debug.WriteLine("Iniciando aplicación...");

                // Crear el servicio de autenticación
                IAuthService authService = new MockAuthService();

                if (authService == null)
                {
                    throw new Exception("No se pudo crear el servicio de autenticación");
                }

                Debug.WriteLine("Servicio de autenticación creado");

                // Crear el ViewModel
                var loginViewModel = new LoginViewModel(authService);

                if (loginViewModel == null)
                {
                    throw new Exception("No se pudo crear el LoginViewModel");
                }

                Debug.WriteLine("LoginViewModel creado");

                // Crear la vista
                var loginView = new LoginView(loginViewModel);

                if (loginView == null)
                {
                    throw new Exception("No se pudo crear el LoginView");
                }

                Debug.WriteLine("LoginView creado");

                // Establecer la página principal
                MainPage = new NavigationPage(loginView);

                Debug.WriteLine("MainPage configurada exitosamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al inicializar la app: {ex.Message}");
                Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                // Mostrar una página de error
                MainPage = new ContentPage
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = 30,
                        Spacing = 20,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                Text = "Error al iniciar la aplicación",
                                FontSize = 24,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center,
                                TextColor = Colors.Red
                            },
                            new Label
                            {
                                Text = ex.Message,
                                FontSize = 16,
                                HorizontalOptions = LayoutOptions.Center,
                                HorizontalTextAlignment = TextAlignment.Center
                            },
                            new Label
                            {
                                Text = "Detalles técnicos:",
                                FontSize = 14,
                                FontAttributes = FontAttributes.Bold,
                                Margin = new Thickness(0, 20, 0, 0)
                            },
                            new ScrollView
                            {
                                Content = new Label
                                {
                                    Text = ex.StackTrace,
                                    FontSize = 12,
                                    FontFamily = "Courier"
                                },
                                HeightRequest = 200
                            },
                            new Button
                            {
                                Text = "Reintentar",
                                BackgroundColor = Colors.Green,
                                TextColor = Colors.White,
                                Margin = new Thickness(0, 20, 0, 0),
                                Command = new Command(() =>
                                {
                                    // Reintentar inicialización
                                    try
                                    {
                                        IAuthService authService = new MockAuthService();
                                        var loginViewModel = new LoginViewModel(authService);
                                        var loginView = new LoginView(loginViewModel);
                                        MainPage = new NavigationPage(loginView);
                                    }
                                    catch (Exception retryEx)
                                    {
                                        Debug.WriteLine($"Error al reintentar: {retryEx.Message}");
                                    }
                                })
                            }
                        }
                    }
                };
            }
        }
    }
}