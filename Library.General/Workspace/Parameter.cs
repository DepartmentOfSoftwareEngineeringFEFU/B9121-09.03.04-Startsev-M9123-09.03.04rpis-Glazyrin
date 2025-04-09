using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Workspace
{
    public class Parameter
    {
        public string id { get; set; }

        public int level { get; set; }

        public Parameter(string id)
        {
            this.id = id;

        }

        public Parameter(string id, int level)
        {
            this.id = id;
            this.level = level;
        }
    }
}
