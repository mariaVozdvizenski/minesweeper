using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MenuSystem
{
    public class Menu
    {
        private readonly int _menuLevel;
        private Dictionary<string, MenuItem> _menuItems = new Dictionary<string, MenuItem>();
        
        private const string MenuCommandExit = "X";
        private const string MenuCommandReturnToPrevious = "P";
        private const string MenuCommandReturnToMain = "M";
        
        public Menu(int level = 0)
        {
            _menuLevel = level;
        }

        public Dictionary<string, MenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                if (_menuLevel >= 2)
                {
                    _menuItems.Add(MenuCommandReturnToPrevious, 
                        new MenuItem(){Title = "Return to Previous Menu"});
                }
                if (_menuLevel >= 1)
                {
                    _menuItems.Add(MenuCommandReturnToMain, 
                        new MenuItem(){Title = "Return to Main Menu"});
                }
                _menuItems.Add(MenuCommandExit, 
                    new MenuItem(){ Title = "Exit"});
            }
        }

        public string Run()
        {
            var command = "";
            do
            {
                Console.Clear();
                Console.WriteLine("MineSweeper");
                Console.WriteLine("========================");

                foreach (var menuItem in MenuItems)
                {
                    Console.Write(menuItem.Key);
                    Console.Write(" ");
                    Console.WriteLine(menuItem.Value.Title);
                }
                
                Console.WriteLine("----------");
                Console.Write(">");

                command = Console.ReadLine()?.Trim().ToUpper() ?? "";

                var returnCommand = "";

                if (MenuItems.ContainsKey(command))
                {
                    var menuItem = MenuItems[command];
                    
                    if (menuItem.CommandToExecute != null)
                    {
                        returnCommand = menuItem.CommandToExecute();
                        break;
                    }
                }

                
                if (returnCommand == MenuCommandExit)
                {
                    command = MenuCommandExit;
                }

                if (returnCommand == MenuCommandReturnToMain)
                {
                    if (_menuLevel != 0)
                    {
                        command = MenuCommandReturnToMain;
                    }
                }
                
                
            } while (command != MenuCommandExit && 
                     command != MenuCommandReturnToMain && 
                     command != MenuCommandReturnToPrevious);

            
            return command;
        }

    }
}