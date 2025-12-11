using CommunityToolkit.Mvvm.ComponentModel;
using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1.ViewModels
{
    public partial class UsersListViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private List<User> users;

        public UsersListViewModel(IAuthService authService)
        {
            _authService = authService;
            LoadUsers();
        }

        private async void LoadUsers()
        {
            var mock = _authService as MockAuthService;
            Users = mock?.GetAllMockUsers();
        }
    }
}
