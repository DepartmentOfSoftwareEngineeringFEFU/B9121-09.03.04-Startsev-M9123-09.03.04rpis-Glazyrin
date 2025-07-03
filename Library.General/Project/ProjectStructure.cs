using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Project
{
    public class Namespace
    {
        public string Name { get; set; }
        public List<Level> Levels { get; set; } = new List<Level>();

        public Namespace(string name)
        {
            Name = name;
        }
    }

    public class Level
    {
        public int LevelNumber { get; set; }
        public List<LogicModule> Modules { get; set; } = new List<LogicModule>();

        public Level(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }

}
