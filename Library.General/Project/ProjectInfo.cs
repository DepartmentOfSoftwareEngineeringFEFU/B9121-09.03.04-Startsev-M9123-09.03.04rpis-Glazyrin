using System.Collections.Generic;
using Library.General.User;

namespace Library.General.Project
{
    public class ProjectInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public string Owner { get; set; }
        public string Picture { get; set; }
        public bool IsCompleted { get; set; }

        // Новые свойства для пользователей и модулей
        public List<UserInfo> Users { get; set; } = new List<UserInfo>();
        public List<LogicModule> Modules { get; set; } = new List<LogicModule>();

        // Конструктор для локальных проектов
        public ProjectInfo(string name, string path, string date)
        {
            Name = name;
            Path = path;
            Date = date;
            IsCompleted = false;
        }

        // Конструктор для серверных проектов (базовый)
        public ProjectInfo(string name, string owner, string date, string picture)
        {
            Name = name;
            Owner = owner;
            Date = date;
            Picture = picture;
        }

        public ProjectInfo(int id, string name, string owner, string date, string picture)
        {
            Id = id;
            Name = name;
            Owner = owner;
            Date = date;
            Picture = picture;
        }

        // Расширенный конструктор для серверных проектов
        public ProjectInfo(string name, string owner, string date, string picture,
                         List<UserInfo> users, List<LogicModule> modules)
            : this(name, owner, date, picture)
        {
            Users = users ?? new List<UserInfo>();
            Modules = modules ?? new List<LogicModule>();
        }

        // Метод для добавления пользователя
        public void AddUser(UserInfo user)
        {
            if (user != null)
            {
                Users.Add(user);
            }
        }

        // Метод для добавления модуля
        public void AddModule(LogicModule module)
        {
            if (module != null)
            {
                Modules.Add(module);
            }
        }
    }
}