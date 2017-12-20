using System.Collections.Generic;

namespace osu_to_Intralism.Utils
{
    public class Pattern
    {
        public List<int> Timings { get; set; }
        public List<int> Directions { get; set; }
    }

    public enum Direction
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
    }
}
