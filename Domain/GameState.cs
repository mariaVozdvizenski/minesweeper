using System;

namespace Domain
{
    public class GameState
    {
        public int Id { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }

        public string BoardStateJson { get; set; } = default!;
    }
}