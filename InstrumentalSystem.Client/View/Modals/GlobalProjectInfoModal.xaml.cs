using InstrumentalSystem.Client.Logic.Config;
using Library.General.Project;
using Library.General.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace InstrumentalSystem.Client.View.Modals
{
    public partial class GlobalProjectInfoModal : UserControl
    {
        private ProjectInfo _info;
        private static readonly HttpClient _httpClient = new HttpClient();

        public GlobalProjectInfoModal(ProjectInfo project)
        {
            _info = project;
            InitializeComponent();

            InitializeAsync().ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            
            ModalHeader.Content = _info.Name;
            ProjectNameLabel.Text = _info.Name;
            LastEditedLabel.Text = _info.Date;

            await LoadProjectVersionsAsync();

            ModuleList.ItemsSource = _info.Modules ?? new List<LogicModule>();
            UserList.ItemsSource = _info.Users ?? new List<UserInfo>();
            this.DataContext = this;


            // InitializeMockVersions(); 
        }

        private List<ProjectVersion> _versions = new List<ProjectVersion>();
        public List<ProjectVersion> Versions
        {
            get => _versions;
            set
            {
                _versions = value;
            }
        }

        private List<UserInfo> _currentVersionUsers = new List<UserInfo>();
        public List<UserInfo> CurrentVersionUsers
        {
            get => _currentVersionUsers;
            set
            {
                _currentVersionUsers = value;
                UserList.ItemsSource = _currentVersionUsers;
            }
        }

        private List<LogicModule> _currentVersionModules = new List<LogicModule>();
        public List<LogicModule> CurrentVersionModules
        {
            get => _currentVersionModules;
            set
            {
                _currentVersionModules = value;
                ModuleList.ItemsSource = _currentVersionModules;
            }
        }


        private ProjectVersion _selectedVersion;
        public ProjectVersion SelectedVersion
        {
            get => _selectedVersion;
            set
            {
                _selectedVersion = value;
                if (_selectedVersion != null)
                {
                    LastEditedLabel.Text = $"Последние изменения: {_selectedVersion.Date:dd.MM.yyyy}";
                    UpdateDataForSelectedVersion();
                }
            }
        }

        private async void UpdateDataForSelectedVersion()
        {
            if (_selectedVersion == null)
            {
                CurrentVersionUsers = new List<UserInfo>();
                CurrentVersionModules = new List<LogicModule>();
                return;
            }

            // Обновляем пользователей
            if (_selectedVersion.users != null)
            {
                try
                {
                    var users = new List<UserInfo>();
                    foreach (var userDto in _selectedVersion.users)
                    {
                        users.Add(new UserInfo(userDto.Name, userDto.Picture));
                    }
                    CurrentVersionUsers = users;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке пользователей версии: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentVersionUsers = new List<UserInfo>();
                }
            }

            // Обновляем модули
            if (_selectedVersion.moduls != null)
            {
                try
                {
                    var modules = new List<LogicModule>();
                    foreach (var moduleId in _selectedVersion.moduls)
                    {
                        var module = await LoadModule(moduleId);
                        if (module != null)
                        {
                            modules.Add(new LogicModule(module.Name));
                        }
                    }

                    CurrentVersionModules = GetDistinctModulesByName(modules);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке модулей версии: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentVersionModules = new List<LogicModule>();
                }
            }
        }

        private void InitializeMockVersions()
        {
            // Создаем моковые данные для версий
            Versions = new List<ProjectVersion>
            {
                new ProjectVersion
                {
                    Id = 1,
                    Name = "Текущая версия",
                    Date = DateTime.Now,
                    Description = "Актуальная версия проекта"
                },
                new ProjectVersion
                {
                    Id = 2,
                    Name = "Версия 1.0",
                    Date = DateTime.Now.AddDays(-7),
                    Description = "Первая стабильная версия"
                },
                new ProjectVersion
                {
                    Id = 3,
                    Name = "Черновик",
                    Date = DateTime.Now.AddDays(-14),
                    Description = "Черновая версия проекта"
                }
            };

            SelectedVersion = Versions[0]; // Устанавливаем первую версию по умолчанию
        }

        private async Task LoadProjectVersionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://127.0.0.1:8095/projects/{_info.Id}/versions");
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Не удалось загрузить версии проекта", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var versionDtos = JsonConvert.DeserializeObject<List<ServerProjectVersionDto>>(content);

                var versions = new List<ProjectVersion>();
                foreach (var dto in versionDtos)
                {
                    versions.Add(new ProjectVersion
                    {
                        Id = dto.id,
                        Name = dto.commitName,
                        Date = dto.date,
                        Description = dto.commitName,
                        users = dto.users,
                        moduls = dto.moduls
                    });
                }

                versions.Sort((a, b) => b.Date.CompareTo(a.Date));

                Versions = versions;
                SelectedVersion = versions.Count > 0 ? versions[0] : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке версий: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var project = await LoadServerProject(_info);
                var editor = new Editor(project);
                editor.Show();
                this.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке проекта: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<Project> LoadServerProject(ProjectInfo projectInfo)
        {
            var projectResponse = await _httpClient.GetAsync($"http://127.0.0.1:8095/projects/{projectInfo.Id}");
            projectResponse.EnsureSuccessStatusCode();

            var projectJson = await projectResponse.Content.ReadAsStringAsync();
            var serverProject = JsonConvert.DeserializeObject<ServerProject>(projectJson);

            var project = new Project(serverProject.Name);

            foreach (var moduleId in serverProject.Moduls)
            {
                var module = await LoadModule(moduleId);
                var levelNamespace = new LogicModuleNamespace(module.Name);
                project.Add(levelNamespace);
                var logicModule = new LogicModule($"Уровень {module.Level}");
                logicModule.SetContent(module.Content);
                if (module != null)
                {
                    project.Add(levelNamespace.Name, logicModule);
                }
            }


            project.Id = projectInfo.Id;

            return project;
        }

        private async Task<ServerModule> LoadModule(int moduleId)
        {
            var response = await _httpClient.GetAsync($"http://127.0.0.1:8095/modules/{moduleId}");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ServerModule>(content);
        }

        private void VersionButton_Click(object sender, RoutedEventArgs e)
        {
            if (VersionDropdown.IsOpen)
            {
                VersionDropdown.IsOpen = false;
            }
            else
            {
                var button = (Button)sender;

                VersionDropdown.PlacementTarget = button;
                VersionDropdown.Placement = PlacementMode.Bottom;

                VersionDropdown.IsOpen = true;

                VersionDropdown.StaysOpen = false;
                VersionDropdown.AllowsTransparency = true;
                VersionDropdown.PopupAnimation = PopupAnimation.Slide;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!VersionDropdown.IsMouseOver && !VersionButton.IsMouseOver)
            {
                VersionDropdown.IsOpen = false;
            }
        }

        private List<LogicModule> GetDistinctModulesByName(List<LogicModule> modules)
        {
            var seenNames = new HashSet<string>();
            var uniqueModules = new List<LogicModule>();

            foreach (var module in modules)
            {
                if (!seenNames.Contains(module.Name))
                {
                    seenNames.Add(module.Name);
                    uniqueModules.Add(module);
                }
            }

            return uniqueModules;
        }
    }
    

    // DTO для версий проекта
    public class ServerProjectVersionDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<ProjectUserDto> users { get; set; } 
        public string owner { get; set; }             
        public DateTime date { get; set; }
        public string path { get; set; }               
        public List<int> moduls { get; set; }         
        public string commitName { get; set; }
    }


    public class ServerProject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Moduls { get; set; } = new List<int>();
    }

    public class ServerModule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Content { get; set; }
    }

    public class ProjectVersion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        public List<int> moduls { get; set; }

        public List<ProjectUserDto> users { get; set; }

        public override string ToString() => $"{Name} ({Date:dd.MM.yyyy})";
    }
}