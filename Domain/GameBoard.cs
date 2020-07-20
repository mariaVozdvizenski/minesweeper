namespace Domain
{
    public class GameBoard
    {
        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MineCount { get; set; }

        public string SaveGameName { get; set; } = default!;
        public string? PanelsListJson { get; set; }
        public GameStatus Status { get; set; }
    }
}