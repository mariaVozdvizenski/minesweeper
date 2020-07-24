using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DAL;
using Domain;

namespace GameEngine
{
    public class GameBoardEngine
    {
        private readonly AppDbContext _context;

        public int Id { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public ICollection<Panel>? GameBoardPanels { get; set; }

        public GameStatus GameStatus { get; set; }
        
        public GameBoardEngine(AppDbContext context)
        {
            _context = context;
        }
        public GameBoard CreateNewGameBoard(int height, int width, int mineCount)
        {
            var newPanels = new List<Panel>();

            var id = 1;
            
            for (var i = 1; i <= height; i++)
            {
                for (var j = 1; j <= width; j++)
                {
                    newPanels.Add(new Panel()
                    {
                        Id = id,
                        X = j,
                        Y = i
                    });
                    id++;
                }
            } 
            var gameBoard = new GameBoard()
            {
                Height = height,
                MineCount = mineCount,
                PanelsListJson = JsonSerializer.Serialize(newPanels),
                Status = GameStatus.InProgress,
                Width = width
            };
            
            GameStatus = GameStatus.InProgress;
            GameBoardPanels = newPanels;
            Height = height;
            Width = width;
            
            return gameBoard;  
        }

        public GameBoard GetGameBoardFromDb(int id)
        {
            var gameBoard = _context.GameBoards.FirstOrDefault(e => e.Id == id);
            
            Height = gameBoard.Height;
            Width = gameBoard.Width;
            GameStatus = gameBoard.Status;
            Id = gameBoard.Id;
            
            DeserializeGameBoardPanels(gameBoard);
            
            return gameBoard;
        }

        public void DeserializeGameBoardPanels(GameBoard gameBoard)
        {
            GameBoardPanels = JsonSerializer.Deserialize<ICollection<Panel>>(gameBoard.PanelsListJson);
        }

        public void AddGameBoardToDb(GameBoard gameBoard)
        {
            SerializeGameBoardPanels(gameBoard);
            _context.GameBoards.Add(gameBoard);
            _context.SaveChangesAsync();
        }

        public void UpdateGameBoard(GameBoard gameBoard)
        {
            SerializeGameBoardPanels(gameBoard);
            _context.GameBoards.Update(gameBoard);
            _context.SaveChangesAsync();
        }

        private void SerializeGameBoardPanels(GameBoard gameBoard)
        {
            gameBoard.PanelsListJson = JsonSerializer.Serialize(GameBoardPanels);
        }
        
        public void FirstMove(int x, int y, Random rand, GameBoard gameBoard)
        {
            //For any board, take the user's first revealed panel + any neighbors of that panel to X depth, and mark them as unavailable for mine placement.
            var depth = 0.125 * Width; //12.5% (1/8th) of the board width becomes the depth of unavailable panels
            var neighbors = GetNeighbors(x, y, (int)depth); //Get all neighbors to specified depth
            neighbors.Add(GameBoardPanels.First(panel => panel.X == x && panel.Y == y)); //Don't place a mine in the user's first move!

            //Select random panels from set of panels which are not excluded by the first-move rule
            var mineList = GameBoardPanels.Except(neighbors).OrderBy(user => rand.Next()); 
            var mineSlots = mineList.Take(gameBoard.MineCount).ToList().Select(z => new { z.X, z.Y });

            //Place the mines
            foreach (var mineCoord in mineSlots)
            {
                GameBoardPanels.Single(panel => panel.X == mineCoord.X && panel.Y == mineCoord.Y).IsMine = true;
            }

            //For every panel which is not a mine, determine and save the adjacent mines.
            foreach (var openPanel in GameBoardPanels.Where(panel => !panel.IsMine))
            {
                var nearbyPanels = GetNeighbors(openPanel.X, openPanel.Y);
                openPanel.AdjacentMines = nearbyPanels.Count(z => z.IsMine);
            }
        }

        public List<Panel> GetNeighbors(int x, int y)
        {
            return GetNeighbors(x, y, 1);
        }


        public Panel GetPanel(int x, int y)
        {
            try
            {
                return GameBoardPanels.First(panel => panel.X == x && panel.Y == y);
            }
            catch (Exception e)
            {
                Console.WriteLine($"X: {x}, Y: {y}");
                Console.WriteLine(e);
                throw;
            }
        }

        public void UnflagPanel(int x, int y)
        {
            var panel = GetPanel(x, y);
            panel.IsFlagged = false;
        }

        public List<Panel> GetNeighbors(int x, int y, int depth)
        {
            var nearbyPanels = GameBoardPanels.Where(panel => panel.X >= (x - depth) && panel.X <= (x + depth)
                                                                                     && panel.Y >= (y - depth) && panel.Y <= (y + depth));
            var currentPanel = GameBoardPanels.Where(panel => panel.X == x && panel.Y == y);
            return nearbyPanels.Except(currentPanel).ToList();
        }
        
        public void RevealPanel(int x, int y, GameBoard gameBoard)
        {
            //Step 1: Find the Specified Panel
            var selectedPanel = GetPanel(x, y);
            selectedPanel.IsRevealed = true;
            selectedPanel.IsFlagged = false; //Revealed panels cannot be flagged

            //Step 2: If the panel is a mine, game over!
            if (selectedPanel.IsMine)
            {
                RevealMines();
                gameBoard.Status = GameStatus.Failed;
            }

            //Step 3: If the panel is a zero, cascade reveal neighbors
            if (!selectedPanel.IsMine && selectedPanel.AdjacentMines == 0)
            {
                RevealZeros(x, y);
            }

            //Step 4: If this move caused the game to be complete, mark it as such
            if (!selectedPanel.IsMine)
            {
                CompletionCheck(gameBoard);
            }

        }

        public void RevealMines()
        {
            var minePanels = GameBoardPanels.Where(panel => panel.IsMine);
            
            foreach (var panel in minePanels)
            {
                panel.IsRevealed = true;
            }
            
        }
        public void RevealZeros(int x, int y)
        {
            var neighborPanels = GetNeighbors(x, y).Where(panel => !panel.IsRevealed);
            foreach (var neighbor in neighborPanels)
            {
                neighbor.IsRevealed = true;
                if (neighbor.AdjacentMines == 0)
                {
                    RevealZeros(neighbor.X, neighbor.Y);
                }
            }
        }
        
        private void CompletionCheck(GameBoard gameBoard)
        {
            var hiddenPanels = GameBoardPanels.Where(x => !x.IsRevealed).Select(x => x.Id);
            var minePanels = GameBoardPanels.Where(x => x.IsMine).Select(x => x.Id);
            if (!hiddenPanels.Except(minePanels).Any())
            {
                gameBoard.Status = GameStatus.Completed;
            }
        }
        
        public void FlagPanel(int x, int y)
        {
            var panel = GetPanel(x, y);
            if(!panel.IsRevealed)
            {
                panel.IsFlagged = true;
            }
        }
    }
}