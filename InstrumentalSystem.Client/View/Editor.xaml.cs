using InstrumentalSystem.Client.Logic.Config;
using InstrumentalSystem.Client.View.Additional;
using InstrumentalSystem.Client.View.Modal;
using Library.Analyzer.Automata;
using Library.Analyzer.Charts;
using Library.Analyzer.Forest;
using Library.Analyzer.PDL;
using Library.Analyzer.Runtime;
using Library.Analyzer.Tokens;
using Library.General.NameTable;
using Library.General.Project;
using Library.General.Workspace;
using Library.InterfaceConnection.Writers;
using Library.IOSystem.Reader;
using Library.IOSystem.Writer;
using Library.NextLevelGenerator.Creators;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace InstrumentalSystem.Client.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Editor : Window
    {

        private ParseEngine? _engine = default(ParseEngine);
        private IInternalForestNode? _root = default(IInternalForestNode);
        private Project? _project = default(Project);
        private IModuleNameTable _nameTable;
        private List<Parameter> _parameters;

        public Editor(string path)
        {
            InitializeComponent();
            var pathParser = path.Split("\\");
            ClientConfig.Project = ProjectReader.ReadProject($"{path}\\{pathParser[pathParser.Length-1]}.master");
            _project = ClientConfig.Project;
            ProjectNameLabel.Content = _project.Name;
            tvLogicModules.ItemsSource = ClientConfig.Project.Namespaces;
            CodeEditor.IsEnabled = false;
        }

        public void TreeRefresh()
        {
            tvLogicModules.Items.Refresh();
        }

        private void InitializeEngine()
        {
            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "grammar.pdl");
            var content = File.ReadAllText(path);

            string filePath = "C:/Users/denst/OneDrive/Рабочий стол/grammar.txt";

            // Используйте StreamWriter для записи информации в файл
            using (StreamWriter writer = new StreamWriter(filePath, true)) // true для добавления в конец файла
            {
                writer.WriteLine(path);
                writer.WriteLine(content);
            }

            // parse the grammar definition file
            var pdlParser = new PdlParser();
            var definition = pdlParser.Parse(content);

            // create the grammar, parser and scanner for our calculator language
            var grammar = new PdlGrammarGenerator().Generate(definition);
            _engine = new ParseEngine(grammar);
        }

        private string RichTextBoxText(TextBox textBox)
            => textBox.Text;


        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            Console.Document.Blocks.Clear();
            //посмотреть что за логика в этом методе
            InitializeEngine();
            var scanner = new ParseRunner(_engine, RichTextBoxText(CodeEditor), new ConsoleWriter(Console));
            var recognized = false;
            var errorPosition = 0;
            while (!scanner.EndOfStream())
            {
                recognized = scanner.Read();
                if (!recognized)
                {
                    errorPosition = scanner.Position;
                    break;
                }
            }

            var accepted = false;
            if (recognized)
            {
                accepted = scanner.ParseEngine.IsAccepted();
                if (!accepted)
                    errorPosition = scanner.Position;
            }
            //scanner.
            //выяснить, почему у _root значение Start
            if (recognized && accepted)
            {
                _root = _engine?.GetParseForestRootNode();
            }

            if (_root != null)
            {
                _engine?.OutputParseTree();
            }


            string filePathToken = "C:/Users/denst/OneDrive/Рабочий стол/tokens.txt";

            // Используйте StreamWriter для записи информации в файл
            using (StreamWriter writerer = new StreamWriter(filePathToken, true)) // true для добавления в конец файла
            {
                //writer.WriteLine(log._nameTable.ToString());
                foreach (var token in scanner._listOfTokens)
                {
                    ILexeme lexeme = token.Item1;

                    if (token.Item1 is DfaLexeme)
                    {
                        DfaLexeme lex = (DfaLexeme)token.Item1;
                        writerer.WriteLine($"Lexeme:  {lex.Capture.ToString()}");
                    }

                    if (token.Item1 is StringLiteralLexeme)
                    {
                        StringLiteralLexeme lex = (StringLiteralLexeme)token.Item1;
                        writerer.WriteLine($"Lexeme:  {lex.Capture.ToString()}");
                    }
                    List<INormalState> states = token.Item2;

                    // Записываем значение лексемы
                    

                    // Записываем состояния
                    foreach (var state in states)
                    {
                        writerer.WriteLine($"  State: {state.ToString()}");
                    }
                }
            }

            if (Console.Document.Blocks.Count > 0)
                return;

            var log = new NameSearchForestNodeVisitor(new TextBoxWriter(Console));

            if (!(_root is null) )
            {
                string filePathType = "C:/Users/denst/OneDrive/Рабочий стол/name_table.txt";

                // Используйте StreamWriter для записи информации в файл
                string type = _root.GetType().FullName;

                using (StreamWriter writer = new StreamWriter(filePathType, true)) // true для добавления в конец файла
                {
                    writer.WriteLine(type);
                }

                log.Visit((ISymbolForestNode)_root);
                _nameTable = log._nameTable;
                _parameters = log._parameters;

                //Console.AppendText(log._nameTable.ToString());
                Console.AppendText("Успешная компиляция");
            }
            log.Print();

            //если не раскомментировать верхнюю строку, то алгоритм не доходит до конца и не читает последний символ(узел)

            string filePath = "C:/Users/denst/OneDrive/Рабочий стол/name_table.txt";

            // Используйте StreamWriter для записи информации в файл
            using (StreamWriter writer = new StreamWriter(filePath, true)) // true для добавления в конец файла
            {
                writer.WriteLine(log._nameTable.ToString());
            }

        }

        private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            AttachedProperties.box_SizeChanged(sender, null);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue is LogicModule oldModule && CodeEditor.IsEnabled)
            {
                oldModule.SetContent(CodeEditor.Text);
            }

            // Загружаем текст нового модуля
            if (e.NewValue is LogicModule newModule)
            {
                CodeEditor.Clear();
                CodeEditor.AppendText(newModule.Content);
                SelectedModuleNameLabel.Content = newModule.Name;
                CodeEditor.IsEnabled = true;
            }
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var @namespace in _project.Namespaces)
            {
                foreach(var level in @namespace.Levels)
                {
                    level.SetContent(level.Name + "asdas ::: Test");
                }
            }
            var project = new ProjectWriter(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Project"));
            project.WriteProject(_project);
        }

        private void CreateModuleButton_Click(object sender, RoutedEventArgs e)
        {

            if(_nameTable is null && _parameters is null)
            {
                ContentGrid.Children.Add(new ModuleCreationModal(this, default(ModuleNameTable), default(List<Parameter>)));
            }

            if (_nameTable is not null && _parameters is null)
            {
                ContentGrid.Children.Add(new ModuleCreationModal(this, _nameTable, default(List<Parameter>)));
            }

            if (_nameTable is null && _parameters is not null)
            {
                ContentGrid.Children.Add(new ModuleCreationModal(this, default(ModuleNameTable), _parameters));
            }

            if (_nameTable is not null && _parameters is not null)
            {
                ContentGrid.Children.Add(new ModuleCreationModal(this, _nameTable, _parameters));
            }


        }

        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "Выбор проекта";
            openFileDialog.Filter = "Master files (*.master)|*.master";
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
        }

        private void SaveModuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (CodeEditor.IsEnabled && !string.IsNullOrWhiteSpace(CodeEditor.Text))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Сохранение модуля",
                    RestoreDirectory = true,
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var path = saveFileDialog.FileName;

                    try
                    {
                        File.WriteAllText(path, CodeEditor.Text);
                        MessageBox.Show("Модуль успешно сохранён.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении модуля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Нет данных для сохранения.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveProjectButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
