using MauiApp1.Services;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class UsersListView : ContentPage
    {
        private readonly IAuthService _authService;

        public UsersListView()
        {
            InitializeComponent();
            _authService = ServiceHelper.GetService<IAuthService>();

            LoadUsers();
        }

        private async void LoadUsers()
        {
            var currentUser = await _authService.GetCurrentUserAsync();

            // Obtener usuarios simulados (menos el usuario actual)
            var users = _authService.GetAllMockUsers()
                                    .Where(u => u.Id != currentUser.Id)
                                    .ToList();

            // Crear lista de items de UI
            var listItems = users.Select(u => new UserRoleItem(u, _authService, RefreshList))
                                 .ToList();

            UsersCollectionView.ItemsSource = listItems;
        }

        // 🔄 Refrescar interfaz después de cambiar un rol
        private void RefreshList()
        {
            LoadUsers();
        }
    }

    // =============================== //
    //  CLASE HELPER PARA CADA USUARIO //
    // =============================== //
    public class UserRoleItem
    {
        public int Id => User.Id;
        public string Name => User.Name;
        public string Email => User.Email;
        public string CurrentRole => User.Roles.FirstOrDefault()?.Name ?? "user";
        public Color RoleButtonColor => CurrentRole == "admin" ? Colors.OrangeRed : Colors.SteelBlue;
        public string RoleButtonLabel => CurrentRole == "admin" ? "Cambiar a user" : "Cambiar a admin";

        public Command ChangeRoleCommand { get; }

        private readonly User User;
        private readonly IAuthService _authService;
        private readonly Action _refreshCallback;

        public UserRoleItem(User user, IAuthService authService, Action refreshCallback)
        {
            User = user;
            _authService = authService;
            _refreshCallback = refreshCallback;

            ChangeRoleCommand = new Command(async () =>
            {
                string newRole = CurrentRole == "admin" ? "user" : "admin";

                bool success = await _authService.UpdateUserRoleAsync(User.Id, newRole);

                if (success)
                {
                    // actualizar modelo local
                    User.Roles.Clear();
                    User.Roles.Add(new Role
                    {
                        Id = newRole == "admin" ? 2 : 1,
                        Name = newRole
                    });

                    await Application.Current.MainPage.DisplayAlert(
                        "Rol actualizado",
                        $"{User.Name} ahora es '{newRole}'.",
                        "OK"
                    );

                    // 🔄 Refrescar la UI
                    _refreshCallback?.Invoke();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se pudo actualizar el rol.",
                        "OK"
                    );
                }
            });
        }
    }
}
