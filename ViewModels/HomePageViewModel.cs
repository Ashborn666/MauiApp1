using CommunityToolkit.Mvvm.ComponentModel;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels
{
    public partial class HomePageViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private User currentUser;

        public HomePageViewModel(IAuthService authService)
        {
            _authService = authService;
            LoadUser();
        }

        private async void LoadUser()
        {
            CurrentUser = await _authService.GetCurrentUserAsync();
        }
    }
}
