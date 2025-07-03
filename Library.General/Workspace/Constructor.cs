using Library.Analyzer.Forest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Workspace
{
    public class Constructor : NameValue
    {
        public string id { get; set; }

        public Constructor(String id, List<ITokenForestNode> dfaLexemes)
            : base(dfaLexemes)
        {
            this.id = id;
        }

    }
}
