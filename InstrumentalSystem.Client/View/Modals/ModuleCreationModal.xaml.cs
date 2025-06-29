using InstrumentalSystem.Client.Logic.Config;
using InstrumentalSystem.Client.Logic.Task;
using InstrumentalSystem.Client.View.Pages.ModuleCreation;
using Library.Analyzer.Automata;
using Library.Analyzer.Collections;
using Library.Analyzer.Forest;
using Library.Analyzer.Tokens;
using Library.General.NameTable;
using Library.General.Project;
using Library.General.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace InstrumentalSystem.Client.View.Modal
{
    /// <summary>
    /// Логика взаимодействия для ModuleCreationModal.xaml
    /// </summary>
    public partial class ModuleCreationModal : UserControl
    {
        private List<ModuleCreationTask> _tasks;
        private List<BaseNameElement> _undefinedNames;
        private List<BaseNameElement> _taskNames;
        private List<Function> _functions;
        private ModuleNameTable _nameTable;
        private List<Parameter> _parameters;
        private List<String> _generatedConstructors;
        private int _count;
        private int _additionalCount;
        private LogicModuleNamespace _moduleNamespace;
        private LogicModule _parent;
        private Editor _parentWindow;
        private Dictionary<string, List<string>> _addedValues;
        StringBuilder _sorts = new StringBuilder();
        public ModuleCreationModal(Editor parentWindow, IModuleNameTable? nameTable, List<Parameter>? parameters)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
            if (nameTable != null)
                if (nameTable is ModuleNameTable moduleNameTable)
                {
                    _undefinedNames = moduleNameTable.GetUndefidedNames();
                    _nameTable = moduleNameTable;
                }

            if (parameters != null)
            {
                _parameters = parameters;
            } else _parameters = new List<Parameter> ();

            _taskNames = new List<BaseNameElement>();          
            _tasks = new List<ModuleCreationTask>();
            _functions = new List<Function>();
            _generatedConstructors = new List<string>();
            _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.SetModuleName));
            _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.Uses));
            _count = 0;
            _additionalCount = 2;
            TaskList.ItemsSource = _tasks;

            SetNextContent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            //if (_tasks[_count].Equals("Задать значение"))
            //{
            //    values.Add(_taskList[_count].ID, new List<string>());
            //    var text = ((SortSetValuePage)TaskPage.Content).NamesTextBox.Text;
            //    foreach (var value in text.Split("\n"))
            //    {
            //       values[_taskList[_count].ID].Add(value);
            //    }
            //}
            if (_tasks.Count < 3)
                UpdateAdditionalTasks();
            else
                UpdateTasks();


            _count++;
            if (_count == _tasks.Count)
            {
                NextButton.IsEnabled = false;
                if (!(_parent is null))
                {
                    _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").AddContent(
                        $"{_sorts.ToString()}" +
                        $"End;\n");


                    if(_parameters.Count != 0)
                    {
                        foreach (var param in _parameters)
                        {
                            if (param.level > 0)
                            {
                                _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").AddContent($"Param {param.id}: {param.level};\n");
                            }
                            else
                            {
                                _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").AddContent($"Param {param.id};");
                            }
                        }
                    }

                    if(_generatedConstructors.Count != 0)
                    {
                        foreach (var constructor in _generatedConstructors)
                        {
                            _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").AddContent(constructor);
                        }
                    }

                    ClientConfig.Project.Add(_moduleNamespace.Name, _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}"));
                }
                else
                {
                    ClientConfig.Project.Add(_moduleNamespace);
                }
                    
                _parentWindow.TreeRefresh();
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetNextContent();
            }
            TaskList.Items.Refresh();
            
        }

        private void SetNextContent()
        {
            if (_count > 0)
                _tasks[_count - 1].InProgress = false;
            Page? page = default;
            switch (_tasks[_count].TaskType) 
            {
                case ModuleCreationTaskType.SetModuleName:
                    page = new SetNamePage();
                    break;
                case ModuleCreationTaskType.Uses:
                    page = new ModuleInicPage(ClientConfig.Project);
                    break;
                case ModuleCreationTaskType.BaseOn:
                    page = new ParentModulePage(ClientConfig.Project);
                    break;
                case ModuleCreationTaskType.SortSetValue:
                    page = new SortSetValuePage(_taskNames[_count -_additionalCount]);
                    break;
                case ModuleCreationTaskType.SortSetName:
                    page = new SortNamePage(_taskNames[_count - _additionalCount]);
                    break;
                case ModuleCreationTaskType.SortDefineValue:
                    page = new SortDefineValuePage(_taskNames[_count - _additionalCount]);
                    break;
                case ModuleCreationTaskType.ConstructConfirmation:
                    var coupledSort = DefineCoupledSortForConstructorByPrefix(_taskNames[_count - _additionalCount]);
                    var expectedType = coupledSort.Value.ToString();
                    var nextLevelTerm = coupledSort.ID;
                    var confirmationPage = new ConstructCreationalConfirmation(_taskNames[_count - _additionalCount], expectedType);

                    confirmationPage.ConstructorProcessed += (constructor, terms) =>
                    {
                        if (terms != null && terms.Count > 0)
                        {
                            var termBuilder = new StringBuilder($"{nextLevelTerm} = ");
                            termBuilder.Append("{");
                            foreach (var term in terms)
                            {
                                termBuilder.Append($"{term.termName}, ");
                            }
                            var termin = $" {termBuilder.ToString().Substring(0, termBuilder.ToString().Length - 2)}" + "};\n";
                            _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}")
                                    .AddContent(termin);

                            if (_taskNames[_count - _additionalCount].Prefix.PrefixCouples.Count == 1)
                            {
                                foreach (var term in terms)
                                {
                                    _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}")
                                        .AddContent($" {term.termName} == {constructor.ID}({term.argument});\n");
                                }
                            } else
                            {
                                foreach (var term in terms)
                                {
                                    _generatedConstructors.Add($"Constructor {term.termName} == {constructor.ID}({term.argument});\n");
                                }
                            }

                            
                        } 
                        NextButton_Click(null, null);
                    };

                    page = confirmationPage;
                    break;
            }

            TaskPage.Content = page;
        }

        private void UpdateAdditionalTasks()
        {
            if (_tasks[_count].TaskType == ModuleCreationTaskType.SetModuleName)
            {
                if (TaskPage.Content is SetNamePage namePage)
                {
                    if (namePage.isEmpty.IsChecked != true)
                    {
                        _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.BaseOn)); 
                        _additionalCount++;
                    }
                    else
                    {
                        _moduleNamespace = new LogicModuleNamespace(namePage.NameTextBox.Text);
                        _moduleNamespace.AddLevel(new LogicModule($"Уровень {namePage.LevelTextBox.Text}"));
                        #pragma warning disable CS8602
                        _moduleNamespace.GetLevel($"Уровень {namePage.LevelTextBox.Text}").SetContent(
                            $"Module {namePage.NameTextBox.Text}: {namePage.LevelTextBox.Text};\n" +
                            $"Begin\n" +
                            $" \n" +
                            $"End;\n");
                        #pragma warning restore CS8602
                    }
                }
            }
        }

        private void UpdateTasks()
        {
            switch (_tasks[_count].TaskType)
            {
                case ModuleCreationTaskType.BaseOn:
                    BaseOnPageType();
                    SetModule();
                    break;
                case ModuleCreationTaskType.SortSetValue:
                    SortSetValuePageType();
                    break;
                case ModuleCreationTaskType.SortDefineValue:
                    SortDefineValuePageType();
                    break;
                case ModuleCreationTaskType.SortConstructCreational:
                    SortConstructCreationalPageType();
                    break;
            }
        }

        private void SortConstructCreationalPageType()
        {
            if (TaskPage.Content is SortConstructCreational constructCreationalPage)
            {
                var constructors = _nameTable.GetConstructors();
            }
        }

        private void SortSetValuePageType()
        {
            if (TaskPage.Content is SortSetValuePage sortValuePage)
            {
                var currentSortId = sortValuePage._name.ID;
                var isSortsNeeded = IsNeedInDefineSorts(currentSortId);
                StringBuilder builder = new StringBuilder($" {currentSortId} = ");
                builder.Append("{");
                foreach (var line in sortValuePage.NamesTextBox.Text.Split("\n"))
                {
                    if (line.Length > 0)
                    {
                        //проверка типа

                        builder.Append($"{line.Replace("\r","")}, ");
                        if (isSortsNeeded.Count != 0)
                            foreach (var sort in isSortsNeeded)
                            {
                                if (sort.Value is MainNameValue mainNameValue)
                                {
                                    if (mainNameValue.GetUndefinedType() == UndefinedType.Undefined_Sets)
                                    {
                                        _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.SortDefineValue));
                                        _taskNames.Add(new BaseNameElement(
                                            NameElementType.MainName, new List<PrefixCouple>(), line, 
                                            new List<ITokenForestNode>(sort.Value.Value)));
                                    }
                                    else
                                    {
                                        _sorts.AppendLine($" Sort {line.Replace("\r", "")}: {sort.Value.ToString()};");
                                    }
                                }
                            }
                    }
                }

                String termin = $"{builder.ToString().Substring(0, builder.ToString().Length - 2)}" + "};\n";

                bool isFunction = false;

                foreach (var func in _functions)
                {
                    if (func.sourceElement.ID == currentSortId)
                    {
                        func.sourceAsTerm = termin.Substring(3 + currentSortId.Length, termin.Length - 5 - currentSortId.Length);
                        isFunction = true;
                    }

                    if (func.destinationElement.ID == currentSortId)
                    {
                        func.destinationAsTerm = termin.Substring(3 + currentSortId.Length, termin.Length - 5 - currentSortId.Length);
                        isFunction = true;
                    }

                    if (func.sourceAsTerm != null && func.destinationAsTerm != null)
                    {
                        _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").AddContent(func.GetStringAsContent());
                        return;
                    }
                }

                if (!isFunction)
                {
                    _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").AddContent(termin);
                }
               
            }
        }

        private void SortDefineValuePageType()
        {
            if (TaskPage.Content is SortDefineValuePage sortValuePage)
            {
                _sorts.AppendLine($" Sort {sortValuePage._name.ID.Replace("\r", "")}: {(String)sortValuePage.TypesComboBox.SelectedItem};");
            }
        }

        private List<BaseNameElement> IsNeedInDefineSorts(string ID)
        {
            var list = new List<BaseNameElement>();
            foreach (var element in _undefinedNames)
            {
                if (element.IsNeedToDefine(ID))
                    list.Add(element);
            }
            return list;
        }

        private void BaseOnPageType()
        {
            if (TaskPage.Content is ParentModulePage parentPage)
            {
                if (parentPage.ModuleList.SelectedItem is LogicModule module)
                {
                    var parse = module.Name.Split("|");
                    var @namespace = ClientConfig.Project.GetNamespace(parse[0]);
                    _parent = @namespace.GetLevel(parse[1]);

                    _moduleNamespace = new LogicModuleNamespace(parse[0]);
                    _moduleNamespace.AddLevel(new LogicModule($"Уровень {_parent.GetLevel() - 1}"));
                    _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").SetContent(
                        $"Module {parse[0]}: {_parent.GetLevel() - 1};\n" +
                        $"Base {parse[0]}_{_parent.GetLevel()};\n" +
                        $"Begin\n"
                        );
                }
            }
        }

        private void SetModule()
        {

            bool isHaveMainValueWithTypeSetString = false;

            UniqueList<BaseNameElement>? completedTasks = new UniqueList<BaseNameElement>();

            var currentLevelParameters = GetCurrentLevelParameters();

            _parameters.RemoveAll(parameter => currentLevelParameters.Contains(parameter));

            var mainBaseNameElements = _nameTable.GetMainNameValuesAsBaseNameElements();

            //List<BaseNameElement> implications = mainBaseNameElements
            //    .Where(baseElement =>
            //        baseElement.Value is MainNameValue mainNameValue &&
            //        mainNameValue.Value.Any(node =>
            //            node.Token.TokenType.Id == "LOGIC_RELATION_IMPLICATION"
            //        )
            //    )
            //    .ToList();

            //foreach (var implicationElement in implications)
            //{
            //    if (implicationElement.Value is MainNameValue mainNameValue)
            //    {
            //        var implication = ParseImplication(mainNameValue);
            //        if (implication != null && isParamInImplication(implication.Value.Source, implication.Value.Destination, currentLevelParameters))
            //        {
            //            var src = mainBaseNameElements.FirstOrDefault(x => x.ID == implication.Value.Source.Token.Capture.ToString());
            //            var dst = mainBaseNameElements.FirstOrDefault(x => x.ID == implication.Value.Destination.Token.Capture.ToString());

            //            if (src != null && dst != null)
            //            {
            //                _functions.Add(new Function(
            //                    id: implicationElement,
            //                    source: src,
            //                    destination: dst
            //                ));
            //            }

            //        }
            //    }
            //}

            //if(_functions.Count > 0)
            //{
            //    isHaveMainValueWithTypeSetString = true;
            //}

            //foreach (var func in _functions)
            //{
            //    _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.SortSetValue));
            //    _taskNames.Add(func.sourceElement);
            //    completedTasks.Add(func.sourceElement);
            //    _undefinedNames.Remove(func.sourceElement);

            //    _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.SortSetValue));
            //    _taskNames.Add(func.destinationElement);
            //    completedTasks.Add(func.destinationElement);
            //    _undefinedNames.Remove(func.destinationElement);

            //    completedTasks.Add(func.parentElement);
            //}

            foreach (var parameter in currentLevelParameters)
            {
                {
                    var currentLevelBasedCouples = GetCurrentLevelBasedCoupleForGenerationWithParameter(completedTasks, parameter);

                    var constructors = _nameTable.GetConstructors();

                    foreach (var task in _undefinedNames)
                    {
                        foreach (var basedCouple in currentLevelBasedCouples)
                        {

                            if (task.ID.Equals(basedCouple.Value[0].Token.Capture.ToString())
                                && task.Value is MainNameValue mainNameValue
                                && mainNameValue.GetUndefinedType() == UndefinedType.Set_String)
                            {
                                isHaveMainValueWithTypeSetString = true;
                                _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.SortSetValue));
                                _taskNames.Add(task);
                                completedTasks.Add(task);
                            }
                        }

                        foreach (var constructor in constructors)
                        {
                            //если конструктор юзался то удалить из таблицы имен
                            //вручную добавлять конструкторы

                            //вычленить первоприоритетный префикс

                            if (constructor.Prefix.PrefixCouples.Count == 0)
                            {
                                continue;
                            }

                            var outerPrefix = constructor.Prefix.PrefixCouples[0].getRightPartAsString();

                            if (outerPrefix.Equals(parameter.id) && outerPrefix.Equals(task.ID))
                            {
                                _tasks.Add(new ModuleCreationTask(ModuleCreationTaskType.ConstructConfirmation));
                                _taskNames.Add(constructor);
                                if(!task.Equals(constructor))
                                {
                                    completedTasks.Add(task);
                                    completedTasks.Add(constructor);
                                }
                                
                            }
                        }
                    }
                }
            }

            

                //все понятия и таски, которые должны унаследоваться (не входят в completedTasks)

            var targetLevel = _moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}");

            foreach (var task in _nameTable.Elements)
            {
                //подумать как тут условия прописать
                //if ((!isHaveMainValueWithTypeSetString || !completedTasks.Contains(task)) && task.NameElementType is not NameElementType.Constructor) 
                //{
                //    targetLevel.AddContent(task.GetStringAsContent());
                //}
                if ((!isHaveMainValueWithTypeSetString && !completedTasks.Contains(task)) || (!completedTasks.Contains(task) && task.NameElementType is not NameElementType.Constructor))
                {
                    if (task.NameElementType is NameElementType.Constructor)
                    {
                        _generatedConstructors.Add(task.GetStringAsContent());
                        continue;
                    }

                    targetLevel.AddContent(task.GetStringAsContent());
                }
            }

            //_moduleNamespace.GetLevel($"Уровень {_parent.GetLevel() - 1}").SetContent()


            //параметры


            //конструкторы

            //limit
        }

        private List<Parameter> GetCurrentLevelParameters()
        {
            List<Parameter> parameters = new List<Parameter>();

            if (_parameters.Count == 0)
            {
                return parameters;
            }

            return _parameters
                .Where(p => p.level == _parent.GetLevel())
                .ToList();
        }

        private List<IValue> GetCurrentLevelBasedCoupleForGenerationWithParameter(UniqueList<BaseNameElement> tasks, Parameter currentLevelParameter)
        {
            var couples = _undefinedNames
                .Where(task => task.Prefix.PrefixCouples.Count > 0 && task.NameElementType == NameElementType.MainName)
                .SelectMany(task =>
                {
                    var matchingCouples = task.Prefix.PrefixCouples
                        .Where(couple => couple.getRightPartAsString().Equals(currentLevelParameter.id))
                        .ToList();

                    if (matchingCouples.Any())
                    {
                        tasks.Add(task);
                    }

                    return matchingCouples.Select(couple => couple.RightPart);
                })
                .ToList();

            return couples;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private BaseNameElement DefineCoupledSortForConstructorByPrefix(BaseNameElement element)
        {
            if(element.NameElementType != NameElementType.Constructor) 
            {
                return null;
            }

            var rightSort = element.Prefix.PrefixCouples[0].getRightPartAsString();

            var mainElements = _nameTable.GetMainNameValuesAsBaseNameElements();

            foreach (var mainElement in mainElements)
            {
                if (mainElement.ID.Equals(rightSort))
                {
                    return mainElement;
                }
            }

            return null;
        }

        private bool isParamInImplication(ITokenForestNode source, ITokenForestNode destination, List<Parameter> currentLevelParameters)
        {
            string sourceParamName = source.Token.Capture.ToString();
            string destParamName = destination.Token.Capture.ToString();

            var sourceParam = currentLevelParameters.FirstOrDefault(p => p.id == sourceParamName);
            var destParam = currentLevelParameters.FirstOrDefault(p => p.id == destParamName);

            if(sourceParam != null && destParam != null)
            {
                return true;
            }

            return false;
        }

        public static (ITokenForestNode Source, ITokenForestNode Destination)? ParseImplication(MainNameValue mainNameValue)
        {
            var nodes = mainNameValue.Value;
            for (int i = 0; i < nodes.Count - 2; i++)
            {
                if (nodes[i + 1].Token.TokenType.Id == "LOGIC_RELATION_IMPLICATION")
                {
                    var source = nodes[i]; 
                    var destination = nodes[i + 2];  
                    return (source, destination);
                }
            }
            return null;  
        }
    }
}
