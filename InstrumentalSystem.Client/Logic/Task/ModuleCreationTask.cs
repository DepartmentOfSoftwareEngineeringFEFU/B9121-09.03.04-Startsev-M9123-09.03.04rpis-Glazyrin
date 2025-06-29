using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstrumentalSystem.Client.Logic.Task
{
    public class ModuleCreationTask
    {
        public ModuleCreationTaskType TaskType { get; private set; }
        public bool InProgress { get; set; }

        public string TaskName
        {
            get
            {
                return TaskType switch
                {
                    ModuleCreationTaskType.SetModuleName => "Установить имя модуля",
                    ModuleCreationTaskType.Uses => "Используемые модули",
                    ModuleCreationTaskType.BaseOn => "Базовый модуль",
                    ModuleCreationTaskType.SortSetName => "Установить имя сорта",
                    ModuleCreationTaskType.SortDefineValue => "Определить значение сорта",
                    ModuleCreationTaskType.SortSetValue => "Задать значение сорта",
                    ModuleCreationTaskType.SortConstructCreational => "Создать конструктор сорта",
                    ModuleCreationTaskType.ConstructConfirmation => "Подтверждение конструктора",
                    _ => TaskType.ToString()
                };
            }
        }

        public ModuleCreationTask(ModuleCreationTaskType type)
        {
            TaskType = type;
            InProgress = true;
        }
    }
}
