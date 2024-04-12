using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
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
