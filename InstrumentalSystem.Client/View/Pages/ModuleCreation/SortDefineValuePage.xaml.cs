﻿using Library.Analyzer.Forest;
using Library.General.NameTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstrumentalSystem.Client.View.Pages.ModuleCreation
{
    /// <summary>
    /// Логика взаимодействия для SortDefineValuePage.xaml
    /// </summary>
    public partial class SortDefineValuePage : Page
    {
        public BaseNameElement _name;
        private List<String> _list;

        public bool isFunction = false;

        public SortDefineValuePage(BaseNameElement name)
        {
            InitializeComponent();
            _name = name;
            _list = new List<String>();
            foreach (var element in _name.Value.Value)
            {
                if ((element.Token.TokenType.Id.Equals("LOGIC_RELATION_IMPLICATION"))) {
                    isFunction = true;
                    break;
                }
            }

            if (!isFunction)
            {
                foreach (var element in _name.Value.Value)
                    if ((element.Token.TokenType.Id.Equals("STRING_S") ||
                        element.Token.TokenType.Id.Equals("INT_S") ||
                        element.Token.TokenType.Id.Equals("REAL_S")) &&
                        !_list.Contains(element.Token.Capture.ToString()))
                    {
                        _list.Add(element.Token.Capture.ToString());
                    }
            }


            Task.Content = $"{_name.Prefix.ToString().Replace("\r", "")} Sort {_name.ID.Replace("\r", "")} : {_name.Value.ToString()}";
            TypesComboBox.ItemsSource = _list;
            if (_list.Count == 1) TypesComboBox.SelectedItem = _list[0];
            TypesComboBox.Visibility = Visibility.Visible;
        }

    }
}
