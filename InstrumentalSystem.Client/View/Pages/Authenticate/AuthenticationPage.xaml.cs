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

            // Подписываемся на событие кнопки "Войти"
            _parent.NextButton.Click += LoginButton_Click;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные с формы
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
                // Создаем объект для отправки
                var loginData = new
                {
                    login = login,
                    passwor = password // Обратите внимание на опечатку в API (passwor вместо password)
                };

                // Сериализуем в JSON
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправляем POST-запрос
                var response = await _httpClient.PostAsync(LoginApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Читаем ответ
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    // Здесь можно сохранить ID пользователя и выполнить другие действия
                    MessageBox.Show($"Успешный вход! ID пользователя: {result.id}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Закрываем модальное окно после успешного входа
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
            // Отписываемся от события перед переходом на другую страницу
            _parent.NextButton.Click -= LoginButton_Click;
            _parent.AuthenticationFrame.Content = new RegistrationPage(_parent);
        }

        // Класс для десериализации ответа
        private class LoginResponse
        {
            public int id { get; set; }
        }
    }
}