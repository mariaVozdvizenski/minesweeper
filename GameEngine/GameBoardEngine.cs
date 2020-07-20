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
        private readonly GameBoard _gameBoard;
        private readonly AppDbContext _context;
        
        private ICollection<Panel>? GameBoardPanels { get; set; }
        public GameBoardEngine(GameBoard gameBoard, AppDbContext context)
        {
            _gameBoard = gameBoard;
            _context = context;
        }

        public void DeserializeGameBoardPanels()
        {
            GameBoardPanels = JsonSerializer.Deserialize<ICollection<Panel>>(_gameBoard.PanelsListJson);
        }

        public void AddGameBoardToDb()
        {
            SerializeGameBoardPanels();
            _context.GameBoards.Add(_gameBoard);
            _context.SaveChangesAsync();
        }

        public void UpdateGameBoard()
        {
            SerializeGameBoardPanels();
            _context.GameBoards.Update(_gameBoard);
            _context.SaveChangesAsync();
        }

        public void SerializeGameBoardPanels()
        {
            _gameBoard.PanelsListJson = JsonSerializer.Serialize(GameBoardPanels);
        }

        public void InitializeGameBoard()
        {
            var newPanels = new List<Panel>();

            var id = 1;
            
            for (var i = 1; i <= _gameBoard.Height; i++)
            {
                for (var j = 1; j <= _gameBoard.Width; j++)
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
            _gameBoard.PanelsListJson = JsonSerializer.Serialize(newPanels);
            _gameBoard.Status = GameStatus.InProgress;
            GameBoardPanels = newPanels;  //Let's start the game!
        }

        public void FirstMove(int x, int y, Random rand)
        {
            //For any board, take the user's first revealed panel + any neighbors of that panel to X depth, and mark them as unavailable for mine placement.
            var depth = 0.125 * _gameBoard.Width; //12.5% (1/8th) of the board width becomes the depth of unavailable panels
            var neighbors = GetNeighbors(x, y, (int)depth); //Get all neighbors to specified depth
            neighbors.Add(GameBoardPanels.First(panel => panel.X == x && panel.Y == y)); //Don't place a mine in the user's first move!

            //Select random panels from set of panels which are not excluded by the first-move rule
            var mineList = GameBoardPanels.Except(neighbors).OrderBy(user => rand.Next()); 
            var mineSlots = mineList.Take(_gameBoard.MineCount).ToList().Select(z => new { z.X, z.Y });

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
            return GameBoardPanels.First(panel => panel.X == x && panel.Y == y);
        }

        public List<Panel> GetNeighbors(int x, int y, int depth)
        {
            var nearbyPanels = GameBoardPanels.Where(panel => panel.X >= (x - depth) && panel.X <= (x + depth)
                                                                            && panel.Y >= (y - depth) && panel.Y <= (y + depth));
            var currentPanel = GameBoardPanels.Where(panel => panel.X == x && panel.Y == y);
            return nearbyPanels.Except(currentPanel).ToList();
        }
        
        public void RevealPanel(int x, int y)
        {
            //Step 1: Find the Specified Panel
            var selectedPanel = GameBoardPanels.First(panel => panel.X == x && panel.Y == y);
            selectedPanel.IsRevealed = true;
            selectedPanel.IsFlagged = false; //Revealed panels cannot be flagged

            //Step 2: If the panel is a mine, game over!
            if (selectedPanel.IsMine)
            {
                RevealMines();
                _gameBoard.Status = GameStatus.Failed;
            }

            //Step 3: If the panel is a zero, cascade reveal neighbors
            if (!selectedPanel.IsMine && selectedPanel.AdjacentMines == 0)
            {
                RevealZeros(x, y);
            }

            //Step 4: If this move caused the game to be complete, mark it as such
            if (!selectedPanel.IsMine)
            {
                CompletionCheck();
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
        
        private void CompletionCheck()
        {
            var hiddenPanels = GameBoardPanels.Where(x => !x.IsRevealed).Select(x => x.Id);
            var minePanels = GameBoardPanels.Where(x => x.IsMine).Select(x => x.Id);
            if (!hiddenPanels.Except(minePanels).Any())
            {
                _gameBoard.Status = GameStatus.Completed;
            }
        }
        
        public void FlagPanel(int x, int y)
        {
            var panel = GameBoardPanels.First(z => z.X == x && z.Y == y);
            if(!panel.IsRevealed)
            {
                panel.IsFlagged = true;
            }
        }
    }
}