using Library.General.NameTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Workspace
{
    public class Function
    {
        public BaseNameElement parentElement { get; set; }

        public BaseNameElement sourceElement {get; set; }

        public BaseNameElement destinationElement { get; set;}

        public String sourceAsTerm { get; set; }

        public String destinationAsTerm { get; set; }

        public Function(BaseNameElement id, BaseNameElement source, BaseNameElement destination)
        {
            this.parentElement = id;
            this.sourceElement = source;
            this.destinationElement = destination;
        }   

        public String GetStringAsContent()
        {
            return $" Sort {parentElement.ID}: {sourceAsTerm} => {destinationAsTerm};\n";
        }
    }
}
