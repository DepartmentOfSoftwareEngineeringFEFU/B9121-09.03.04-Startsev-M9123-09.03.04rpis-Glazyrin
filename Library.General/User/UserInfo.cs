using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.User
{
    public class UserInfo
    {
        public string Name { get; private set; }

        public string Picture { get; private set; }
        
        public UserInfo(string name)
        {
            Name = name;
        }

        public UserInfo(string name, string picture) : this(name)
        {
            Picture = picture;
        }
    }
}
