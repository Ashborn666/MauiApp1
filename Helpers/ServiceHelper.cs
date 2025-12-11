using Microsoft.Maui.Controls;

namespace MauiApp1;

public static class ServiceHelper
{
    public static T GetService<T>() =>
        Current.GetService<T>();

    public static IServiceProvider Current =>
        Application.Current.Handler.MauiContext.Services;
}