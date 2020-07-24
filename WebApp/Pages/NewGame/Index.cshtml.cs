using System;
using System.Linq;
using DAL;
using Domain;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.NewGame
{
    public class Index : PageModel
    {
        [BindProperty] public GameBoard GameBoard { get; set;} = new GameBoard();

        private readonly AppDbContext _appDbContext;
        
        public Index (AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public void OnGet()
        {
            
        }

        public IActionResult OnPost()
        {
            GameBoard!.MineCount = 10;
            
            var gameBoardEngine = new GameBoardEngine(_appDbContext);
            var gameBoard = gameBoardEngine.CreateNewGameBoard(GameBoard.Height, GameBoard.Width, GameBoard.MineCount);
            gameBoard.SaveGameName = GameBoard.SaveGameName;

            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            if (_appDbContext.GameBoards.Any(e => e.SaveGameName == GameBoard!.SaveGameName))
            {
                ModelState.AddModelError("GameBoard.SaveGameName", "A save game with this name already exists.");
                return Page();
            }
            gameBoardEngine.AddGameBoardToDb(gameBoard);
            return RedirectToPage("/PlayGame/Index", new {id = gameBoard.Id});
        }
    }
}