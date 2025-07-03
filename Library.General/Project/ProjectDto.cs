using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Project
{
    public class ProjectUserDto
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
    }

    public class ServerModuleDto
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
    }

    public class ServerProjectDto
    {
        public int id { get; set; }
        public string Name { get; set; }
        public List<ServerModuleDto> Modules { get; set; }
    }

    public class ServerProjectInfoDto
    {
        public int id { get; set; }
        public string Name { get; set; }
        public List<ProjectUserDto> users { get; set; }
        public string Owner { get; set; }
        public string Date { get; set; }
        public string Path { get; set; }
        public List<int> Moduls { get; set; }
    }
}