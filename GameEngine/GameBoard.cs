using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine
{
    public class GameBoard
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MineCount { get; set; }
        public ICollection<Panel> Panels { get; set; }
        public GameStatus Status { get; set; }
        
        public GameBoard(int width, int height, int mines)
        {
            Width = width;
            Height = height;
            MineCount = mines;
            Panels = new List<Panel>();

            int id = 1;
            for(int i = 1; i <= height; i++)
            {
                for (int j = 1; j <= width; j++)
                {
                    Panels.Add(new Panel(id, j, i));
                    id++;
                }
            }

            Status = GameStatus.InProgress; //Let's start the game!
        }
        
        public void FirstMove(int x, int y, Random rand)
        {
            //For any board, take the user's first revealed panel + any neighbors of that panel to X depth, and mark them as unavailable for mine placement.
            var depth = 0.125 * Width; //12.5% (1/8th) of the board width becomes the depth of unavailable panels
            var neighbors = GetNeighbors(x, y, (int)depth); //Get all neighbors to specified depth
            neighbors.Add(Panels.First(panel => panel.X == x && panel.Y == y)); //Don't place a mine in the user's first move!

            //Select random panels from set of panels which are not excluded by the first-move rule
            var mineList = Panels.Except(neighbors).OrderBy(user => rand.Next()); 
            var mineSlots = mineList.Take(MineCount).ToList().Select(z => new { z.X, z.Y });

            //Place the mines
            foreach (var mineCoord in mineSlots)
            {
                Panels.Single(panel => panel.X == mineCoord.X && panel.Y == mineCoord.Y).IsMine = true;
            }

            //For every panel which is not a mine, determine and save the adjacent mines.
            foreach (var openPanel in Panels.Where(panel => !panel.IsMine))
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
                return Panels.First(panel => panel.X == x && panel.Y == y);
            }
            catch (Exception e)
            {
                 Console.WriteLine($"Exception {e}. Variables x - {x} and y - {y}");
                 return new Panel(1, 1, 1);
            }
        }

        public List<Panel> GetNeighbors(int x, int y, int depth)
        {
            var nearbyPanels = Panels.Where(panel => panel.X >= (x - depth) && panel.X <= (x + depth)
                                                                            && panel.Y >= (y - depth) && panel.Y <= (y + depth));
            var currentPanel = Panels.Where(panel => panel.X == x && panel.Y == y);
            return nearbyPanels.Except(currentPanel).ToList();
        }
        
        public void RevealPanel(int x, int y)
        {
            //Step 1: Find the Specified Panel
            var selectedPanel = Panels.First(panel => panel.X == x && panel.Y == y);
            selectedPanel.IsRevealed = true;
            selectedPanel.IsFlagged = false; //Revealed panels cannot be flagged

            //Step 2: If the panel is a mine, game over!
            if (selectedPanel.IsMine)
            {
                RevealMines();
                Status = GameStatus.Failed;
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
            var minePanels = Panels.Where(panel => panel.IsMine);
            
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
            var hiddenPanels = Panels.Where(x => !x.IsRevealed).Select(x => x.ID);
            var minePanels = Panels.Where(x => x.IsMine).Select(x => x.ID);
            if (!hiddenPanels.Except(minePanels).Any())
            {
                Status = GameStatus.Completed;
            }
        }
        
        public void FlagPanel(int x, int y)
        {
            var panel = Panels.First(z => z.X == x && z.Y == y);
            if(!panel.IsRevealed)
            {
                panel.IsFlagged = true;
            }
        }
    }
}