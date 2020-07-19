using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.SymbolStore;
using GameEngine;
using GameUI;
using MenuSystem;

namespace Program
{
    class Program
    {
        private static readonly Menu _menu0 = new Menu(0);

        
        static void Main(string[] args)
        {
            Console.Clear();
            _menu0.MenuItems = new Dictionary<string, MenuItem>
            {
                {
                    "S", new MenuItem
                    {
                        Title = "Start game",
                        CommandToExecute = TestGame
                    }
                },
                
                {
                    "L", new MenuItem
                    {
                        Title = "Load Game",
                        CommandToExecute = null
                    }
                }
            };
            _menu0.Run();
        }

        private static string TestGame()
        {
            var userHeight = 0;
            var userWidth = 0;
            var userCanceledGame = false;
            
            (userHeight, userCanceledGame, _) = GetUserIntInput("Enter board height", 8, 64, 0);
            ClearConsoleIfUserCanceled(userCanceledGame);

            (userWidth, userCanceledGame, _) = GetUserIntInput("Enter board width", 8, 64, 0);
            ClearConsoleIfUserCanceled(userCanceledGame);
            
            var game = new GameBoard(userWidth, userHeight, 10);
            
            ConsoleUI.PrintBoard(game);
            
            var userXint = 0;
            var userYint = 0;
            var userCanceled = false;
            var plantFlag = false;

            (userYint, userCanceled, _) = GetUserIntInput("Enter Y coordinate", 1, game.Height, 0);
            ClearConsoleIfUserCanceled(userCanceled);
            
            (userXint, userCanceled, _) = GetUserIntInput("Enter X coordinate", 1, game.Width, 0);
            ClearConsoleIfUserCanceled(userCanceled);

            game.FirstMove(userXint,userYint, new Random());
            game.RevealPanel(userXint,userYint);
            
            do
            {
                Console.Clear();
                ConsoleUI.PrintBoard(game);
                
                (userYint, userCanceled, plantFlag) = GetUserIntInput("Enter Y coordinate", 1, game.Height, 0, "F");
                ClearConsoleIfUserCanceled(userCanceled);
                if (plantFlag)
                {
                    PlantFlag(game);
                }
                else
                {
                    (userXint, userCanceled, _) = GetUserIntInput("Enter X coordinate", 1, game.Width, 0);
                    ClearConsoleIfUserCanceled(userCanceled);
                    game.RevealPanel(userXint, userYint);
                }
                
            } while (game.Status == GameStatus.InProgress);

            switch (game.Status)
            {
                case GameStatus.Completed:
                    Console.WriteLine("GAME WON");
                    break;
                
                case GameStatus.Failed:
                    ConsoleUI.PrintBoard(game);
                    Console.WriteLine("GAME LOST!");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return "wee";
        }

        static (int result, bool wasCanceled, bool flagPlanted) GetUserIntInput(string prompt, int min, int max,
            int? cancelIntValue = null, string flagButton = null, string cancelStrValue = "")
        {
            do
            {
                Console.WriteLine(prompt);
                if (flagButton != null)
                {
                    Console.WriteLine("To put a flag, press 'F'");
                }
                if (cancelIntValue.HasValue || !string.IsNullOrWhiteSpace(cancelStrValue))
                {
                    Console.WriteLine($"To cancel input enter: {cancelIntValue}" +
                                      $"{(cancelIntValue.HasValue && !string.IsNullOrWhiteSpace(cancelStrValue) ? " or " : "")}" +
                                      $"{cancelStrValue}");
                }

                Console.Write(">");
                var consoleLine = Console.ReadLine();

                if (consoleLine == cancelStrValue) return (0, true, false);

                if (consoleLine!.ToUpper() == flagButton)
                {
                    return (0, false, true);
                }

                if (int.TryParse(consoleLine, out var userInt))
                {
                    if (userInt < min)
                    {
                        Console.WriteLine($"Min value is {min}!");
                    } 
                    else if (userInt > max)
                    {
                        Console.WriteLine($"Max value is {max}!");
                    }
                    else
                    {
                        return userInt == cancelIntValue ? (userInt, true, false) : (userInt, false, false);
                    }
                }
                else
                {
                    Console.WriteLine($"'{consoleLine}' cant be converted to int value!");
                }
            } while (true);
        }

        static void ClearConsoleIfUserCanceled(bool userCanceled)
        {
            if (!userCanceled) return;
            Console.Clear();
            _menu0.Run();
        }

        static void PlantFlag(GameBoard game)
        {
            var userYint = 0;
            var userXint = 0;
            
            (userYint, _, _) = GetUserIntInput("Enter Flag Y coordinate", 1, game.Height, 0);

            (userXint, _, _) = GetUserIntInput("Enter Flag X coordinate", 1, game.Width, 0);
            
            game.FlagPanel(userXint, userYint);
        }
    }
}