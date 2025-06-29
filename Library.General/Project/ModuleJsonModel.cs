using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Project
{
    public class ModuleJsonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }

        public ModuleJsonModel(int id, string name, string body) { Id = id; Name = name; Body = body; }
    }

}
