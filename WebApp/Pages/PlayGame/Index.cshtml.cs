using System;
using System.Linq;
using DAL;
using Domain;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.PlayGame
{
    public class Index : PageModel
    {
        private readonly AppDbContext _appDbContext;
        public GameBoardEngine GameEngine;
        public GameBoard GameBoard { get; set; }

        [BindProperty]
        public string Coordinates { get; set; }
        

        public Index(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        private (int x, int y) ExtractCoordinatesFromString()
        {
            if (string.IsNullOrWhiteSpace(Coordinates)) return (1, 1);
            Console.WriteLine(Coordinates);
            var coords = Coordinates.Split(",");
            Console.WriteLine(coords);
            return (int.Parse(coords[0]), int.Parse(coords[1]));
        }

        public void OnGet(int id)
        {
            GameBoard = _appDbContext.GameBoards.FirstOrDefault(e => e.Id == id);
            GameEngine = new GameBoardEngine(GameBoard, _appDbContext);
            GameEngine.DeserializeGameBoardPanels();
        }

        public IActionResult OnPostMove()
        {
            var (x, y) = ExtractCoordinatesFromString();
            if (!GameEngine.GameBoardPanels.Any(e => e.IsRevealed))
            {
                GameEngine.FirstMove(x, y, new Random());
            }
            GameEngine.RevealPanel(x, y);
            GameEngine.UpdateGameBoard();
            return RedirectToPage(new {id = GameBoard.Id});
        }

        public IActionResult OnPostFlag()
        {
            var (x, y) = ExtractCoordinatesFromString();
            GameEngine.FlagPanel(x, y);
            GameEngine.UpdateGameBoard();
            return RedirectToPage(new {id = GameBoard.Id});
        }
        
        public static string GetSingleState(Panel panel)
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