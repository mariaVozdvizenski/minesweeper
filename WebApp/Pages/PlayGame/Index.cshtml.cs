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
        public readonly GameBoardEngine GameBoardEngine;
        private GameBoard GameBoard { get; set; }
        
        [BindProperty]
        public string Coordinates { get; set; }
        
        public Index(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            GameBoardEngine = new GameBoardEngine(_appDbContext);
        }

        private (int x, int y) ExtractCoordinatesFromString()
        {
            if (string.IsNullOrWhiteSpace(Coordinates)) return (1, 1);
            Console.WriteLine(Coordinates);
            
            var coords = Coordinates.Split(",");
            Console.WriteLine(coords);
            
            return (int.Parse(coords[0]) + 1, int.Parse(coords[1]) + 1);
        }

        public void OnGet(int id)
        {
            GameBoard = GameBoardEngine.GetGameBoardFromDb(id);
        }

        /*
        public IActionResult OnPostMove(int id)
        {
            var gameBoard = GameBoardEngine.GetGameBoardFromDb(id);
            
            var (x, y) = ExtractCoordinatesFromString();
            
            if (!GameBoardEngine.GameBoardPanels.Any(e => e.IsRevealed))
            {
                GameBoardEngine.FirstMove(x, y, new Random(), gameBoard);
            }
            
            GameBoardEngine.RevealPanel(x, y, gameBoard);
            GameBoardEngine.UpdateGameBoard(gameBoard);
            
            return RedirectToPage(new {id = gameBoard.Id});
        }
        */

        /*public IActionResult OnPostFlag(int id)
        {
            var gameBoard = GameBoardEngine.GetGameBoardFromDb(id);
            
            var (x, y) = ExtractCoordinatesFromString();

            if (GameBoardEngine.GetPanel(x, y).IsFlagged)
            {
                GameBoardEngine.UnflagPanel(x, y);
            }
            else 
            {
                GameBoardEngine.FlagPanel(x, y);
            }
            
            GameBoardEngine.UpdateGameBoard(gameBoard);
            
            return RedirectToPage(new {id = gameBoard.Id});
        }
        */

        public IActionResult OnPostTest(int id, int x, int y, bool isFlagged)
        {
            Console.WriteLine("ugh");
            var jsonResult = new JsonResult(new {id, x, y, isFlagged});
            return jsonResult;
        }
        
        public void OnPostMove(int id, int x, int y)
        {
            var gameBoard = GameBoardEngine.GetGameBoardFromDb(id);

            if (!GameBoardEngine.GameBoardPanels.Any(e => e.IsRevealed))
            {
                GameBoardEngine.FirstMove(x, y, new Random(), gameBoard);
            }
            
            GameBoardEngine.RevealPanel(x, y, gameBoard);
            GameBoardEngine.UpdateGameBoard(gameBoard);
        }

        public void OnPostFlag(int id, int x, int y)
        {
            var gameBoard = GameBoardEngine.GetGameBoardFromDb(id);
            
            if (GameBoardEngine.GetPanel(x, y).IsFlagged)
            {
                GameBoardEngine.UnflagPanel(x, y);
            }
            else if (!GameBoardEngine.GetPanel(x, y).IsRevealed)
            {
                GameBoardEngine.FlagPanel(x, y);
            }
            
            GameBoardEngine.UpdateGameBoard(gameBoard);
        }
        
        
        public string GetSingleState(Panel panel)
        {
            if (panel.IsMine && panel.IsRevealed)
            {
                return "X";
            }

            if (!panel.IsMine && panel.IsRevealed)
            {
                return panel.AdjacentMines.ToString();
            }
            
            return panel.IsFlagged ? "F" : " ";
        }
    }
}