using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DAL;
using Domain;
using GameEngine;
using GameUI;
using MenuSystem;
using Microsoft.EntityFrameworkCore;

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
                        Title = "Start a new game",
                        CommandToExecute = NewGame
                    }
                },
                
                {
                    "L", new MenuItem
                    {
                        Title = "Load Game",
                        CommandToExecute = LoadGame
                    }
                }
            };
            _menu0.Run();
        }

        private static string LoadGame()
        {
            var dbOption = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=/Users/maria/csharp2019fall/Proge/minesweeper/WebApp/app.db").Options;

            var ctx = new AppDbContext(dbOption);
            var query = ctx.GameBoards.ToList();

            Console.Clear();
            Console.WriteLine("Save games:");

            foreach (var game in query)
            {
                Console.WriteLine($"{game.SaveGameName}");
            }
            
            GameBoard gameBoard;
            
            var userXint = 0;
            var userYint = 0;
            var userCanceled = false;
            var plantFlag = false;
            var saveGame = false;

            do
            {
                Console.WriteLine("Enter '0' to cancel input");
                Console.WriteLine("Please enter a save game name:");
                Console.WriteLine(">");
                
                var userInput = Console.ReadLine();
                
                if (ctx.GameBoards.Any(e => e.SaveGameName == userInput))
                {
                    gameBoard = ctx.GameBoards.FirstOrDefault(e => e.SaveGameName == userInput);
                    break;
                }
                if (userInput == "0")
                {
                    userCanceled = true;
                    ClearConsoleIfUserCanceled(userCanceled);
                    return "";
                }
                Console.WriteLine("No such save file found.");
                
            } while (true);
            
            GameBoardEngine gameBoardEngine = new GameBoardEngine(gameBoard, ctx);
            gameBoardEngine.DeserializeGameBoardPanels();
            
            MainGame(gameBoardEngine, gameBoard, ctx, userYint, userXint, userCanceled, saveGame, plantFlag, "updateGame");
            
            return "";
        }

        private static string NewGame()
        {
            var dbOption = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=/Users/maria/csharp2019fall/Proge/minesweeper/WebApp/app.db").Options;
            
            var ctx = new AppDbContext(dbOption);
            
            var userHeight = 0;
            var userWidth = 0;
            var userCanceledGame = false;
            
            (userHeight, userCanceledGame, _, _) = GetUserIntInput("Enter board height", 8, 64, 0);
            ClearConsoleIfUserCanceled(userCanceledGame);

            (userWidth, userCanceledGame, _, _) = GetUserIntInput("Enter board width", 8, 64, 0);
            ClearConsoleIfUserCanceled(userCanceledGame);
            
            
            var game = new GameBoard
            {
                Height = userHeight,
                MineCount = 10,
                Width = userWidth
            };
            
            var gameEngine = new GameBoardEngine(game, ctx);
            gameEngine.InitializeGameBoard();
            
            ConsoleUI.PrintBoard(gameEngine, game);
            
            var userXint = 0;
            var userYint = 0;
            var userCanceled = false;
            var plantFlag = false;
            var saveGame = false;

            (userYint, userCanceled, _, _) = GetUserIntInput("Enter Y coordinate", 1, game.Height, 0);
            ClearConsoleIfUserCanceled(userCanceled);
            
            (userXint, userCanceled, _, _) = GetUserIntInput("Enter X coordinate", 1, game.Width, 0);
            ClearConsoleIfUserCanceled(userCanceled);

            gameEngine.FirstMove(userXint,userYint, new Random());
            gameEngine.RevealPanel(userXint,userYint);
            
            MainGame(gameEngine, game, ctx, userYint, userXint, userCanceled, saveGame, plantFlag, "newGame");
            return "";
        }

        static void MainGame(GameBoardEngine gameEngine, GameBoard game, 
            AppDbContext ctx, int userYint, int userXint, bool userCanceled, bool saveGame, bool plantFlag, string type)
        {
            do
            {
                Console.Clear();
                ConsoleUI.PrintBoard(gameEngine, game);
                
                (userYint, userCanceled, plantFlag, saveGame) = GetUserIntInput("Enter Y coordinate", 1, game.Height, 0, "F", "S");
                if (userCanceled)
                {
                    ClearConsoleIfUserCanceled(userCanceled);
                    break;
                }
                if (plantFlag)
                {
                    PlantFlag(gameEngine, game, plantFlag);
                }
                else if (saveGame)
                {
                    SaveGameIfUserSaved(gameEngine, game, saveGame, ctx, type);
                }
                else
                {
                    (userXint, userCanceled, _, _) = GetUserIntInput("Enter X coordinate", 1, game.Width, 0, null);
                    ClearConsoleIfUserCanceled(userCanceled);
                    gameEngine.RevealPanel(userXint, userYint);
                }
                
            } while (game.Status == GameStatus.InProgress);

            switch (game.Status)
            {
                case GameStatus.Completed:
                    Console.WriteLine("GAME WON");
                    break;
                
                case GameStatus.Failed:
                    ConsoleUI.PrintBoard(gameEngine, game);
                    Console.WriteLine("GAME LOST!");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        static (int result, bool wasCanceled, bool flagPlanted, bool userSaved) GetUserIntInput(string prompt, int min, int max,
            int? cancelIntValue = null, string? flagButton = null, string? saveButton = null, string cancelStrValue = "")
        {
            do
            {
                Console.WriteLine(prompt);
                
                if (flagButton != null)
                {
                    Console.WriteLine($"To put a flag, press '{flagButton}'");
                }

                if (saveButton != null)
                {
                    Console.WriteLine($"To save, press '{saveButton}'");
                }
                
                if (cancelIntValue.HasValue || !string.IsNullOrWhiteSpace(cancelStrValue))
                {
                    Console.WriteLine($"To cancel input enter: {cancelIntValue}" +
                                      $"{(cancelIntValue.HasValue && !string.IsNullOrWhiteSpace(cancelStrValue) ? " or " : "")}" +
                                      $"{cancelStrValue}");
                }

                Console.Write(">");
                var consoleLine = Console.ReadLine();

                if (consoleLine == cancelStrValue) return (0, true, false, false);

                if (consoleLine!.ToUpper() == flagButton)
                {
                    return (0, false, true, false);
                }

                if (consoleLine!.ToUpper() == saveButton)
                {
                    return (0, false, false, true);
                }

                if (int.TryParse(consoleLine, out var userInt))
                {
                    if (userInt < min && userInt != cancelIntValue)
                    {
                        Console.WriteLine($"Min value is {min}!");
                    } 
                    else if (userInt > max)
                    {
                        Console.WriteLine($"Max value is {max}!");
                    }
                    else
                    {
                        return userInt == cancelIntValue ? (userInt, true, false, false) : (userInt, false, false, false);
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

        static void PlantFlag(GameBoardEngine gameBoardEngine, GameBoard gameBoard, bool plantFlag)
        {
            if (!plantFlag) return;
                
            var userYint = 0;
            var userXint = 0;
            
            (userYint, _, _, _) = GetUserIntInput("Enter Flag Y coordinate", 1, gameBoard.Height, 0);

            (userXint, _, _, _) = GetUserIntInput("Enter Flag X coordinate", 1, gameBoard.Width, 0);
            
            gameBoardEngine.FlagPanel(userXint, userYint);
        }

        static void SaveGameIfUserSaved(GameBoardEngine gameBoardEngine, GameBoard gameBoard, bool saveGame, AppDbContext context, string type)
        {
            if (!saveGame) return;

            if (type == "updateGame")
            {
                gameBoardEngine.UpdateGameBoard();
                SavingAnimation();
                return;
            }
            
            Console.Clear();
            Console.WriteLine("Please write a name for the save:");
            Console.WriteLine(">");
            
            var saveGameName = Console.ReadLine();
            
            AddGameToDb(context, gameBoardEngine, gameBoard, saveGameName);
            SavingAnimation();
        }

        private static void SavingAnimation()
        {
            ConsoleSpinner spin = new ConsoleSpinner();
            Console.CursorVisible = false;
            Console.Write("Saving....");
            
            Stopwatch s = new Stopwatch();
            
            s.Start();
            while (s.Elapsed < TimeSpan.FromSeconds(5)) 
            {
                spin.Turn();
            }

            Console.CursorVisible = true;
            s.Stop();
        }

        private static void AddGameToDb
            (AppDbContext context, GameBoardEngine gameBoardEngine, GameBoard gameBoard, string saveGameName)
        {
            while (true)
            {
                if (context.GameBoards.Any(e => e.SaveGameName == saveGameName))
                {
                    Console.WriteLine($"{saveGameName} name already exists. Please pick a new name.");
                    Console.WriteLine(">");
                    saveGameName = Console.ReadLine();
                }
                else
                {
                    gameBoard.SaveGameName = saveGameName;
                    break;
                }
            }
            gameBoardEngine.AddGameBoardToDb();
        }
        
    }
}