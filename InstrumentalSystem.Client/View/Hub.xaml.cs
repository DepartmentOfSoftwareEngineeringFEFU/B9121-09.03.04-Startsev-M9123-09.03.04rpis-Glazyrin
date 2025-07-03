using InstrumentalSystem.Client.View.Modals;
using Library.General.Project;
using Library.General.User;
using Library.IOSystem.Reader;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace InstrumentalSystem.Client.View
{
    public partial class Hub : Window
    {
        private List<ProjectInfo> _projects;
        private List<ProjectInfo> _serverProjects;
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ProjectsApiUrl = "http://127.0.0.1:8095/projects";
        private const string UsersApiUrl = "http://127.0.0.1:8095/users";
        private const string ModulesApiUrl = "http://127.0.0.1:8095/modules";

        public Hub()
        {
            InitializeComponent();
            LoadProjects();
        }

        private async void LoadProjects()
        {
            _projects = ProjectReader.ReadProjectsInfo();
            LocalProjects.ItemsSource = _projects;

            try
            {
                _serverProjects = await FetchServerProjects();
                GlobalProjects.ItemsSource = _serverProjects;
            }
            catch
            {
                _serverProjects = new List<ProjectInfo>();
                GlobalProjects.ItemsSource = _serverProjects;
            }
        }

        private async Task<List<ProjectInfo>> FetchServerProjects()
        {
            var projectsTask = _httpClient.GetAsync(ProjectsApiUrl);
            var usersTask = _httpClient.GetAsync(UsersApiUrl);
            var modulesTask = _httpClient.GetAsync(ModulesApiUrl);

            await Task.WhenAll(projectsTask, usersTask, modulesTask);

            var projectsJson = await projectsTask.Result.Content.ReadAsStringAsync();
            var usersJson = await usersTask.Result.Content.ReadAsStringAsync();
            var modulesJson = await modulesTask.Result.Content.ReadAsStringAsync();

            var projectDtos = JsonConvert.DeserializeObject<List<ServerProjectInfoDto>>(projectsJson);
            var userDtos = JsonConvert.DeserializeObject<List<ProjectUserDto>>(usersJson);
            var moduleDtos = JsonConvert.DeserializeObject<List<ServerModuleDto>>(modulesJson);

            var usersDict = new Dictionary<int, ProjectUserDto>();
            foreach (var user in userDtos)
            {
                usersDict[user.id] = user;
            }

            var modulesDict = new Dictionary<int, ServerModuleDto>();
            foreach (var module in moduleDtos)
            {
                modulesDict[module.id] = module;
            }

            var result = new List<ProjectInfo>();
            foreach (var projectDto in projectDtos)
            {
                var projectUsers = new List<UserInfo>();
                foreach (var userId in projectDto.users)
                {
                    if (usersDict.TryGetValue(userId.id, out var userDto))
                    {
                        projectUsers.Add(new UserInfo(userDto.Name, userId.Picture));
                    }
                }

                var projectModules = new List<LogicModule>();
                foreach (var moduleId in projectDto.Moduls)
                {
                    if (modulesDict.TryGetValue(moduleId, out var moduleDto))
                    {
                        projectModules.Add(new LogicModule(moduleDto.Name));
                    }
                }

                var projectInfo = new ProjectInfo(
                    projectDto.id,
                    projectDto.Name,
                    projectDto.Owner,
                    $"Последние изменения: {projectDto.Date}",
                    projectDto.Path)
                {
                    Users = projectUsers,
                    Modules = projectModules
                };

                result.Add(projectInfo);
            }

            return result;
        }

        public void RefreshProjectList()
        {
            _projects = ProjectReader.ReadProjectsInfo();
            LocalProjects.ItemsSource = _projects;
            LocalProjects.Items.Refresh();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            ContentGrid.Children.Add(new AuthenticationModal(this));
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ContentGrid.Children.Add(new ProjectCreationModal(this));
        }

        private void LocalProjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LocalProjects.SelectedIndex == -1)
                return;
            ContentGrid.Children.Add(new LocalProjectInfoModal(_projects[LocalProjects.SelectedIndex]));
        }

        private void GlobalProjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedProject = GlobalProjects.SelectedItem as ProjectInfo;
            if (selectedProject != null)
            {
                ContentGrid.Children.Add(new GlobalProjectInfoModal(selectedProject));
            }
        }
    }
}
