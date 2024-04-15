using System;
using System.Collections.Generic;

namespace Checkers.Models
{
    public class GameState
    {
        public CheckerColor ActivePlayer { get; set; }
        public int WhitePiecesCount { get; set; }
        public int RedPiecesCount { get; set; }
        public bool AllowMultipleJumps { get; set; }
        public List<BoardSquareState> BoardState { get; set; }
    }
}
