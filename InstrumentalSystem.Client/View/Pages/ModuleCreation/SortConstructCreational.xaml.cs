using System.Windows;
using System.Windows.Controls;
using Library.General.NameTable;
using System.Collections.Generic;
using System;

namespace InstrumentalSystem.Client.View.Pages.ModuleCreation
{
    public partial class SortConstructCreational : Page
    {
        public event Action<List<(string sortName, string argument)>> SortsCreated;
        private readonly BaseNameElement _constructor;

        public string _expectedType { get; }

        public SortConstructCreational(BaseNameElement constructor, string expectedType)
        {
            InitializeComponent();
            _constructor = constructor;
            _expectedType = expectedType;
            DataContext = this;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var createdSorts = new List<(string, string)>();
            var entries = InputTextBox.Text.Split('\n');
            bool hasErrors = false;
            HideMessage();

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;

                var parts = entry.Split('=');
                if (parts.Length == 2)
                {
                    var value = parts[1].Trim();

                    if (!ValidateInputType(value))
                    {
                        ShowMessage($"Ошибка: значение '{value}' не соответствует типу {_expectedType}");
                        hasErrors = true;
                        continue;
                    }

                    createdSorts.Add((parts[0].Trim(), value));
                }
                else
                {
                    ShowMessage($"Ошибка: некорректный формат строки '{entry}'. Используйте формат: Название = Аргумент");
                    hasErrors = true;
                }
            }

            if (!hasErrors && createdSorts.Count > 0)
            {
                ShowMessage("Конструкторы успешно созданы!", false);
                SortsCreated?.Invoke(createdSorts);
            }
            else if (!hasErrors)
            {
                ShowMessage("Нет данных для создания конструкторов");
            }
        }

        private void ShowMessage(string message, bool isError = true)
        {
            MessageTextBlock.Text = message;
            MessageTextBlock.Foreground = isError
                ? System.Windows.Media.Brushes.Red
                : System.Windows.Media.Brushes.Green;

            MessageBorder.Visibility = Visibility.Visible;
        }

        private void HideMessage()
        {
            MessageBorder.Visibility = Visibility.Collapsed;
        }

        private bool ValidateInputType(string input)
        {
            switch (_expectedType)
            {
                case "{}I":
                    return int.TryParse(input, out _);
                case "{}N":
                    return true;
                case "{}R":
                    return double.TryParse(input, out _);
                default:
                    return false;
            }
        }
    }
}