using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    public class BoardSquareState
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public CheckerColor? CheckerColor { get; set; }
        public bool? IsKing { get; set; }
    }
}
