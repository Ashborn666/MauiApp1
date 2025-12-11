using MauiApp1.Services;

namespace MauiApp1.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly IAuthService _authService;

        public HomePage()
        {
            InitializeComponent();
            _authService = ServiceHelper.GetService<IAuthService>();
            LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();

                if (user != null)
                {
                    WelcomeLabel.Text = user.Name;
                    EmailLabel.Text = user.Email;
                    UserIdLabel.Text = user.Id.ToString();

                    // ⭐ Mostrar rol actual
                    string role = user.Roles.FirstOrDefault()?.Name ?? "user";
                    RoleLabel.Text = role;

                    // ⭐ Mostrar panel admin solo si el rol es admin
                    AdminPanel.IsVisible = role == "admin";
                }
                else
                {
                    // Datos desde SecureStorage si algo falla
                    var userName = await SecureStorage.GetAsync("user_name");
                    var userEmail = await SecureStorage.GetAsync("user_email");
                    var userId = await SecureStorage.GetAsync("user_id");

                    WelcomeLabel.Text = userName ?? "Usuario";
                    EmailLabel.Text = userEmail ?? "No disponible";
                    UserIdLabel.Text = userId ?? "No disponible";
                    RoleLabel.Text = "user";

                    AdminPanel.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar información: {ex.Message}", "OK");
            }
        }

        // ⭐ BOTÓN PARA ABRIR LA LISTA DE USUARIOS SOLO SI ES ADMIN
        private async void OnOpenUsersList(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UsersListView());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Cerrar Sesión",
                "¿Seguro que deseas cerrar sesión?",
                "Sí", "No");

            if (!confirm)
                return;

            await _authService.LogoutAsync();

            var loginPage = ServiceHelper.GetService<LoginView>();
            Application.Current.MainPage = new NavigationPage(loginPage);
        }
    }
}
