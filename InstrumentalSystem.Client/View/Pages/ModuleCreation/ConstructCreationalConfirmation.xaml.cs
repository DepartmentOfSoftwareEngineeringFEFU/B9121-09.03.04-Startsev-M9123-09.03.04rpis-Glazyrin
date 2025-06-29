using System.Collections.Generic;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Google.Protobuf.WellKnownTypes;
using Library.General.NameTable;
using Library.General.Workspace;

namespace InstrumentalSystem.Client.View.Pages.ModuleCreation
{
    public partial class ConstructCreationalConfirmation : Page
    {
        public event Action<BaseNameElement, List<(string termName, string argument)>> ConstructorProcessed;
        private readonly BaseNameElement _constructor;
        private string _expectedType;

        public ConstructCreationalConfirmation(BaseNameElement constructor, string expectedType)
        {
            InitializeComponent();
            _constructor = constructor;
            _expectedType = expectedType;
            ConstructorText.Text = GetConstructorAsString();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            var inputPage = new SortConstructCreational(_constructor, _expectedType);

            inputPage.SortsCreated += (sorts) =>
            {
                ConstructorProcessed?.Invoke(_constructor, sorts);
            };

            this.NavigationService.Navigate(inputPage);
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            ConstructorProcessed?.Invoke(_constructor, new List<(string, string)>());
        }

        private string GetConstructorAsString()
        {
            return $"Construct {_constructor.ID} = {_constructor.Value.ToString()};\n";
        }
    }
}
