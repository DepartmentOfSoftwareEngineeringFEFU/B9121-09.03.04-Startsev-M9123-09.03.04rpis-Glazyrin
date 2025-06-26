using System;
using System.Collections.Generic;
using InstrumentalSystem.Server;

namespace InstrumentalSystem.Server
{
    public class MokServerProject : IMokServerProject
    {
        public MokServerProject() 
        { }

        List<string> GetGlobalProjects()
        {
            new List<string> globalProject();

            globalProject.Add("{ Name : Проект_1; Owner : rabbid; Date : 01.07.2022; Path : \\View\\Images\\roles\\roleOwnerIcon.png}");
            globalProject.Add("{ Name : Тестовый_совместный_проект; Owner : wrap; Date : 02.07.2022; Path : \\View\\Images\\roles\\roleEditorIcon.png}");
            globalProject.Add("{ Name : Проверка_Прав; Owner : wrap2; Date : 29.06.2022; Path : \\View\\Images\\roles\\roleViewerIcon.png}");
            globalProject.Add("{ Name : Калькулятор_1; Owner : rabbid; Date : 02.07.2022; Path : \\View\\Images\\roles\\roleOwnerIcon.png}")


            return globalProject;

        }
    }

}