using System;

namespace MenuSystem
{
    public class MenuItem
    {
        public string Title { get; set; } = default!;
        public Func<string>? CommandToExecute { get; set; }
    }
}