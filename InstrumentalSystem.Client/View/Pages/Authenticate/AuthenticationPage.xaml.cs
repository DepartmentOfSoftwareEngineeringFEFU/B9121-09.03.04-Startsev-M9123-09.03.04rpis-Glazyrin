using InstrumentalSystem.Client.View.Modals;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace InstrumentalSystem.Client.View.Pages.Authenticate
{
    public partial class AuthenticationPage : Page
    {
        private AuthenticationModal _parent;
        private readonly HttpClient _httpClient = new HttpClient();
        private const string LoginApiUrl = "https://1a7db8f6-09f6-41d7-9d8d-1e635d52bcc7.mock.pstmn.io/login";

        public AuthenticationPage(AuthenticationModal parent)
        {
            InitializeComponent();
            _parent = parent;
            _parent.NextButton.Content = "Войти";
            _parent.HeaderLabel.Content = "Вход в учетную запись";
            _parent.BackButton.IsEnabled = false;

            _parent.NextButton.Click += LoginButton_Click;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var loginData = new
                {
                    login = login,
                    password = password
                };

                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(LoginApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                    MessageBox.Show($"Успешный вход! ID пользователя: {result.id}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Ошибка при входе. Проверьте логин и пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            _parent.NextButton.Click -= LoginButton_Click;
            _parent.AuthenticationFrame.Content = new RegistrationPage(_parent);
        }

        private class LoginResponse
        {
            public int id { get; set; }
        }
    }
}