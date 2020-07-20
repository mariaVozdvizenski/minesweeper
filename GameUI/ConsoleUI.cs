using System;
using Domain;
using GameEngine;

namespace GameUI
{
    public class ConsoleUI
    {
        private static readonly string _verticalSeparator = "|";
        private static readonly string _horizontalSeparator = "-";
        private static readonly string _centerSeparator = "+";

        public static void PrintBoard(GameBoardEngine gameBoardEngine, GameBoard game)
        {
            Console.Clear();
            for (int yIndex = 0; yIndex < game.Height; yIndex++)
            {
                var line = "";
                
                for (int xIndex = 0; xIndex < game.Width; xIndex++)
                {
                    
                    line = line + " " + GetSingleState(gameBoardEngine.GetPanel(xIndex + 1, yIndex + 1)) + " ";
                    
                    if (xIndex < game.Width - 1)
                    {
                        line = line + _verticalSeparator;
                    }
                }
                
                Console.WriteLine(line);

                if (yIndex < game.Height - 1)
                {
                    line = "";
                    for (int xIndex = 0; xIndex < game.Width; xIndex++)
                    {
                        line = line + _horizontalSeparator+ _horizontalSeparator+ _horizontalSeparator;
                        if (xIndex < game.Height - 1)
                        {
                            line += _centerSeparator;
                        }
                    }
                    Console.WriteLine(line);
                }
            }
        }

        private static string GetSingleState(Panel panel)
        {
            if (panel.IsMine && panel.IsRevealed)
            {
                return "x";
            }

            if (!panel.IsMine && panel.IsRevealed)
            {
                return panel.AdjacentMines.ToString();
            }
            
            return panel.IsFlagged ? "F" : " ";
        }
    }
}