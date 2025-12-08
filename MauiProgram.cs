using Microsoft.Extensions.Logging;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using MauiApp1.Views;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Lato-Bold.ttf", "bold");
                    fonts.AddFont("Lato-Regular.ttf", "medium");
                    fonts.AddFont("email.otf", "AwesomeSolid");
                });

            // Registrar servicios
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // Registrar ViewModels
            builder.Services.AddSingleton<LoginViewModel>();

            // Registrar Views
            builder.Services.AddSingleton<LoginView>();
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}