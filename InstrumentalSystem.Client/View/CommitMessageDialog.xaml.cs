using System.Windows;

namespace InstrumentalSystem.Client.View
{
    public partial class CommitMessageDialog : Window
    {
        public string CommitMessage => CommitMessageTextBox.Text;

        public CommitMessageDialog()
        {
            InitializeComponent();
            CommitMessageTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CommitMessageTextBox.Text))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Сообщение: ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
