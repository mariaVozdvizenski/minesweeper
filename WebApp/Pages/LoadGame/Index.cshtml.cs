using System.Collections.Generic;
using System.Linq;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.LoadGame
{
    public class Index : PageModel
    {
        private readonly AppDbContext _context;
        public ICollection<GameBoard> GameBoards { get; set; }

        public Index(AppDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            GameBoards = _context.GameBoards.ToList();
        }
    }
}