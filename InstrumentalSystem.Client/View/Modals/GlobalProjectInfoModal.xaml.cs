using Library.General.Project;
using Library.General.User;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace InstrumentalSystem.Client.View.Modals
{
    public partial class GlobalProjectInfoModal : UserControl
    {
        public GlobalProjectInfoModal(ProjectInfo project)
        {
            InitializeComponent();

            this.DataContext = project;
            ModalHeader.Content = project.Name;
            ProjectNameLabel.Text = project.Name;
            LastEditedLabel.Text = project.Date;

            // Устанавливаем данные пользователей и модулей
            ModuleList.ItemsSource = project.Modules ?? new List<LogicModule>();
            UserList.ItemsSource = project.Users ?? new List<UserInfo>();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}