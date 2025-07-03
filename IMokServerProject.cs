using System;
using System.Collections.Generic;


namespace InstrumentalSystem.Server

public interface IMokServerProject
{
    List<string> GetGlobalProjects();
    List<string> GetModulesForProject(string projectPath);
    List<string> GetUsersForProject(ProjectInfo project);
}
