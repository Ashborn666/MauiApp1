using MauiApp1.Services;

namespace MauiApp1.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly IAuthService _authService;

        public HomePage()
        {
            InitializeComponent();
            _authService = Handler.MauiContext?.Services.GetService<IAuthService>();
            LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            try
            {
                if (_authService != null)
                {
                    var user = await _authService.GetCurrentUserAsync();

                    if (user != null)
                    {
                        WelcomeLabel.Text = $"{user.FullName}";
                        EmailLabel.Text = user.Email;
                        UserIdLabel.Text = user.Id.ToString();
                    }
                    else
                    {
                        // Si no hay usuario, mostrar datos de SecureStorage
                        var userName = await SecureStorage.GetAsync("user_name");
                        var userEmail = await SecureStorage.GetAsync("user_email");
                        var userId = await SecureStorage.GetAsync("user_id");

                        WelcomeLabel.Text = userName ?? "Usuario";
                        EmailLabel.Text = userEmail ?? "No disponible";
                        UserIdLabel.Text = userId ?? "No disponible";
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar información: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que deseas cerrar sesión?", "Sí", "No");

            if (confirm)
            {
                if (_authService != null)
                {
                    await _authService.LogoutAsync();
                }

                Application.Current.MainPage = new NavigationPage(
                    Handler.MauiContext?.Services.GetService<LoginView>()
                );
            }
        }
    }
}