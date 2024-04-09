using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Checkers
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BoardSquare> Squares { get; set; }

        private int whitePiecesCount;
        private int redPiecesCount;
        private BoardSquare selectedSquare;
        public int WhitePiecesCount
        {
            get => whitePiecesCount;
            set
            {
                whitePiecesCount = value;
                OnPropertyChanged(nameof(WhitePiecesCount));
            }
        }

        public int RedPiecesCount
        {
            get => redPiecesCount;
            set
            {
                redPiecesCount = value;
                OnPropertyChanged(nameof(RedPiecesCount));
            }
        }

        public BoardViewModel()
        {
            Squares = new ObservableCollection<BoardSquare>();
            SetupBoard();
            UpdatePiecesCount();
            InitializeCommands();
        }

        private void UpdatePiecesCount()
        {
            WhitePiecesCount = Squares.Count(s => s.Checker?.Color == CheckerColor.White);
            RedPiecesCount = Squares.Count(s => s.Checker?.Color == CheckerColor.Red);
        }

        public void HandleSquareSelected(BoardSquare square)
        {
            if (selectedSquare != null && IsValidMove(selectedSquare, square))
            {
                MoveChecker(selectedSquare, square);
                ClearSelections();
            }
            else if (selectedSquare != null && square.IsHighlighted)
            {
                MoveChecker(selectedSquare, square);
                ClearSelections();
            }
            else if (square.Checker != null) // Dacă patratelul selectat conține o dama
            {
                ClearSelections();
                selectedSquare = square;
                square.IsSelected = true;
                HighlightPossibleMoves(square);
            }
        }

        public void HighlightPossibleMoves(BoardSquare square)
        {
            ClearSelections();
            var possibleMoves = CalculatePossibleMoves(square);
            foreach (var move in possibleMoves)
            {
                move.IsHighlighted = true;
            }
        }

        private List<BoardSquare> CalculatePossibleMoves(BoardSquare square)
        {
            var moves = new List<BoardSquare>();
            int[] directionRow = { 1, -1 };
            int[] directionCol = { -1, 1 };

            foreach (var rowDir in directionRow)
            {
                foreach (var colDir in directionCol)
                {
                    int newRow = square.Row + rowDir;
                    int newCol = square.Column + colDir;
                    if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                    {
                        var targetSquare = Squares.FirstOrDefault(s => s.Row == newRow && s.Column == newCol);
                        if (targetSquare != null && targetSquare.Checker == null)
                        {
                            moves.Add(targetSquare);
                        }
                    }
                }
            }

            return moves;
        }

        private void ClearSelections()
        {
            foreach (var square in Squares)
            {
                square.IsSelected = false;
                square.IsHighlighted = false;
            }
        }

        private void MoveChecker(BoardSquare fromSquare, BoardSquare toSquare)
        {
            toSquare.Checker = fromSquare.Checker;
            fromSquare.Checker = null;

            UpdatePiecesCount();
            ClearSelections();
        }

        private void InitializeCommands()
        {
            foreach (var square in Squares)
            {
                square.SquareClickCommand = new RelayCommand(param => MoveChecker(square));
            }
        }

        private void MoveChecker(BoardSquare destinationSquare)
        {
            var selectedCheckerSquare = Squares.FirstOrDefault(sq => sq.IsSelected && sq.Checker != null);
            if (selectedCheckerSquare != null && IsValidMove(selectedCheckerSquare, destinationSquare))
            {
                destinationSquare.Checker = selectedCheckerSquare.Checker;
                selectedCheckerSquare.Checker = null;
                UpdatePiecesCount();
            }
        }

        private bool IsValidMove(BoardSquare fromSquare, BoardSquare toSquare)
        {
            if (fromSquare.Checker == null || toSquare.Checker != null) return false;

            int rowDiff = toSquare.Row - fromSquare.Row;
            int colDiff = Math.Abs(toSquare.Column - fromSquare.Column);

            if (!fromSquare.Checker.IsKing) // Dacă piesa nu este regină
            {
                if (fromSquare.Checker.Color == CheckerColor.White)
                {
                    // Piesele albe pot merge doar înainte
                    if (rowDiff == -1 && colDiff == 1)
                        return true;
                }
                else if (fromSquare.Checker.Color == CheckerColor.Red)
                {
                    // Piesele roșii pot merge doar înainte
                    if (rowDiff == 1 && colDiff == 1)
                        return true;
                }

                // Verificare dacă piesa a ajuns la capătul opus al tablei și o transformăm în regină
                if ((fromSquare.Checker.Color == CheckerColor.White && toSquare.Row == 0) ||
                    (fromSquare.Checker.Color == CheckerColor.Red && toSquare.Row == 7))
                {
                    fromSquare.Checker.IsKing = true;
                    return true;
                }
            }
            else // Dacă piesa este regină
            {
                // Regii pot merge în orice direcție cu o singură casetă
                if (colDiff == 1 && Math.Abs(rowDiff) == 1)
                    return true;
            }
            return false;
        }

        private void SetupBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    var isWhiteSquare = (row + column) % 2 == 0;
                    Checker checker = null;

                    // La crearea tablei, toate piesele sunt regi
                    if ((row < 3 && !isWhiteSquare) || (row > 4 && !isWhiteSquare))
                    {
                        checker = new Checker { Color = row < 3 ? CheckerColor.White : CheckerColor.Red, IsKing = true };
                    }

                    var square = new BoardSquare(new RelayCommand(param => HandleSquareSelected(param as BoardSquare)))
                    {
                        Row = row,
                        Column = column,
                        IsWhiteSquare = isWhiteSquare,
                        Checker = checker
                    };

                    Squares.Add(square);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
