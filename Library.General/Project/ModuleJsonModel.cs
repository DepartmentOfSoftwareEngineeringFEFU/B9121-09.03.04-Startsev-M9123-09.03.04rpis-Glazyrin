using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Project
{
    public class ModuleJsonModel
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }

        public ModuleJsonModel(int level, string name, string body) { Level = level; Name = name; Body = body; }
    }

}
