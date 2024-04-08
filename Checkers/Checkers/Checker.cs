using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    public enum CheckerColor
    {
        None,
        White,
        Red
    }

    public class Checker
    {
        public CheckerColor Color { get; set; }
        public bool IsKing { get; set; }
    }
}
