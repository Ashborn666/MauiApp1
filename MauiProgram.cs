using MauiApp1.Services;
using MauiApp1.ViewModels;
using MauiApp1.Views;

namespace MauiApp1;

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
            });

        // AUTH SERVICE COMO SINGLETON (CORRECTO)
        builder.Services.AddSingleton<IAuthService, MockAuthService>();

        // VIEWS QUE SE RECREAN CADA VEZ QUE SE ABREN
        builder.Services.AddTransient<LoginView>();
        builder.Services.AddTransient<RegisterView>();
        builder.Services.AddTransient<ForgotPasswordView>();
        builder.Services.AddTransient<HomePage>();           // TRANSIENT!
        builder.Services.AddTransient<UsersListView>();       // TRANSIENT!

        // VIEWMODELS TAMBIÉN TRANSIENT
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<HomePageViewModel>();
        builder.Services.AddTransient<UsersListViewModel>();

        return builder.Build();
    }
}
