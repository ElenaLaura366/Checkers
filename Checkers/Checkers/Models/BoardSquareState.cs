using System;

namespace Checkers.Models
{
    public class BoardSquareState
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public CheckerColor? CheckerColor { get; set; }
        public bool? IsKing { get; set; }
    }
}
