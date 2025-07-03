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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using System.Configuration;
using System.Net.Http.Json;

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
        private readonly HttpClient _httpClient = new HttpClient();
        private IModuleNameTable _nameTable;
        private List<Parameter> _parameters;

        public Editor(string path)
        {
            InitializeComponent();
            var pathParser = path.Split("\\");
            ClientConfig.Project = ProjectReader.ReadProject($"{path}\\{pathParser[pathParser.Length - 1]}.master");
            _project = ClientConfig.Project;
            ProjectNameLabel.Content = _project.Name;
            tvLogicModules.ItemsSource = ClientConfig.Project.Namespaces;
            CodeEditor.IsEnabled = false;
        }

        public Editor(Project project)
        {
            InitializeComponent();
            ClientConfig.Project = project;
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
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string grammarPath = Path.Combine(projectRoot, "Grammatics", "grammar.pdl");

            if (!File.Exists(grammarPath))
                throw new FileNotFoundException($"Grammar file not found at: {grammarPath}");

            string content = File.ReadAllText(grammarPath);

            //string filePath = "C:/Users/denst/OneDrive/Рабочий стол/grammar.txt";

            //// Используйте StreamWriter для записи информации в файл
            //using (StreamWriter writer = new StreamWriter(filePath, true)) // true для добавления в конец файла
            //{
            //    writer.WriteLine(content);
            //}

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

            //string filePathToken = "C:/Users/denst/OneDrive/Рабочий стол/tokens.txt";

            //// Используйте StreamWriter для записи информации в файл
            //using (StreamWriter writerer = new StreamWriter(filePathToken, true)) // true для добавления в конец файла
            //{
            //    //writer.WriteLine(log._nameTable.ToString());
            //    foreach (var token in scanner._listOfTokens)
            //    {
            //        ILexeme lexeme = token.Item1;

            //        if (token.Item1 is DfaLexeme)
            //        {
            //            DfaLexeme lex = (DfaLexeme)token.Item1;
            //            writerer.WriteLine($"Lexeme:  {lex.Capture.ToString()}");
            //        }

            //        if (token.Item1 is StringLiteralLexeme)
            //        {
            //            StringLiteralLexeme lex = (StringLiteralLexeme)token.Item1;
            //            writerer.WriteLine($"Lexeme:  {lex.Capture.ToString()}");
            //        }
            //        List<INormalState> states = token.Item2;

            //        // Записываем значение лексемы


            //        // Записываем состояния
            //        foreach (var state in states)
            //        {
            //            writerer.WriteLine($"  State: {state.ToString()}");
            //        }
            //    }
            //}

            if (Console.Document.Blocks.Count > 0)
                return;

            var log = new NameSearchForestNodeVisitor(new TextBoxWriter(Console));

            if (!(_root is null))
            {
                //string filePathType = "C:/Users/denst/OneDrive/Рабочий стол/name_table.txt";

                //// Используйте StreamWriter для записи информации в файл
                //string type = _root.GetType().FullName;

                //using (StreamWriter writer = new StreamWriter(filePathType, true)) // true для добавления в конец файла
                //{
                //    writer.WriteLine(type);
                //}

                log.Visit((ISymbolForestNode)_root);
                _nameTable = log._nameTable;
                _parameters = log._parameters;

                //Console.AppendText(log._nameTable.ToString());
                Console.AppendText("Успешная компиляция");
            }
            log.Print();

            //если не раскомментировать верхнюю строку, то алгоритм не доходит до конца и не читает последний символ(узел)

            //string filepath = "c:/users/denst/onedrive/рабочий стол/name_table.txt";

            //// используйте streamwriter для записи информации в файл
            //using (StreamWriter writer = new StreamWriter(filepath, true)) // true для добавления в конец файла
            //{
            //    writer.WriteLine(log._nameTable.ToString());
            //}
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
                foreach (var level in @namespace.Levels)
                {
                    level.SetContent(level.Name + "asdas ::: Test");
                }
            }
            var project = new ProjectWriter(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Project"));
            project.WriteProject(_project);
        }

        private void CreateModuleButton_Click(object sender, RoutedEventArgs e)
        {

            if (_nameTable is null && _parameters is null)
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

        private List<ModuleJsonModel> GetAllModules()
        {
            var modules = new List<ModuleJsonModel>();

            foreach (var namespaceItem in _project.Namespaces)
            {
                if (namespaceItem is LogicModuleNamespace logicNamespace)
                {
                    foreach (var level in logicNamespace.Levels)
                    {
                        int levelValue = 0;
                        Match match = Regex.Match(level.Name, @"\d+");
                        if (match.Success)
                        {
                            levelValue = int.Parse(match.Value);
                        }

                        modules.Add(new ModuleJsonModel
                        (
                            levelValue,
                            logicNamespace.Name,
                            level.Content
                        ));
                    }
                }
            }

            return modules;

            //return JsonConvert.SerializeObject(modules, Formatting.Indented);
        }

        private void AddModuleFromComputer_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл модуля",
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);

                    var moduleInfo = ParseModuleInfo(fileContent);
                    if (moduleInfo == null)
                    {
                        MessageBox.Show("Не удалось распознать структуру модуля в файле",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        return;
                    }

                    var newModule = new LogicModule($"Уровень {moduleInfo.Level}", fileContent);

                    if (ClientConfig.Project != null)
                    {

                        var levelNamespace = ClientConfig.Project.Namespaces
                            .FirstOrDefault(n => n.Name == moduleInfo.Name);

                        if (levelNamespace == null)
                        {
                            levelNamespace = new LogicModuleNamespace(moduleInfo.Name);
                            ClientConfig.Project.Namespaces.Add(levelNamespace);
                        }

                        if (levelNamespace.Levels.Any(m => m.Name == moduleInfo.Name))
                        {
                            MessageBox.Show($"Модуль с именем '{moduleInfo.Name}' уже существует на уровне {moduleInfo.Level}",
                                          "Предупреждение",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Warning);
                            return;
                        }

                        levelNamespace.Levels.Add(newModule);

                        tvLogicModules.Items.Refresh();

                        MessageBox.Show($"Модуль '{moduleInfo.Name}' (уровень {moduleInfo.Level}) успешно добавлен в проект!",
                                      "Успех",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении модуля: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private ModuleInfo ParseModuleInfo(string moduleContent)
        {
            try
            {
                var moduleMatch = Regex.Match(moduleContent, @"Module\s+([a-zA-Z_]\w*)\s*:\s*(\d+)\s*;");
                if (!moduleMatch.Success)
                    return null;

                string name = moduleMatch.Groups[1].Value;
                int level = int.Parse(moduleMatch.Groups[2].Value);

                return new ModuleInfo { Name = name, Level = level };
            }
            catch
            {
                return null;
            }
        }

        private class ModuleInfo
        {
            public string Name { get; set; }
            public int Level { get; set; }
        }


        private async void SaveServerProjectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CommitMessageDialog
                {
                    Owner = this
                };

                if (dialog.ShowDialog() != true)
                {
                    return; 
                }

                string commitMessage = dialog.CommitMessage;
                var modulesJson = GetAllModules();

                List<int> ids = await SaveModulesAsync(modulesJson);



                var payload = new
                {
                    id = _project.Id,
                    commitName = commitMessage,
                    moduls = ids
                };

                string json = System.Text.Json.JsonSerializer.Serialize(payload);

                using (var client = new HttpClient())
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://127.0.0.1:8095/project", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Проект успешно сохранён на сервере", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка: {response.StatusCode}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<List<int>> SaveModulesAsync(List<ModuleJsonModel> modules)
        {
            var savedIds = new List<int>();

            try
            {
                foreach (var module in modules)
                {
                    var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:8095/modules", new
                    {
                        name = module.Name,
                        level = module.Level,
                        content = module.Body
                    });

                    response.EnsureSuccessStatusCode();

                    var savedId = await response.Content.ReadFromJsonAsync<int>();
                    savedIds.Add(savedId);
                }

                return savedIds;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
