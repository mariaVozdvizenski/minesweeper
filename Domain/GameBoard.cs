using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class GameBoard
    {
        public int Id { get; set; }
        
        [Range(8, 64)]
        public int Width { get; set; }
        
        [Range(8, 64)]
        public int Height { get; set; }
        
        public int MineCount { get; set; }

        [MinLength(1)]
        [MaxLength(128)]
        [Display(Name = "Game Name")]
        [Required]
        public string SaveGameName { get; set; } = default!;
        public string? PanelsListJson { get; set; }
        public GameStatus Status { get; set; }
    }
}