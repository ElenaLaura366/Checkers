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
            else
            {
                ClearSelections();
                selectedSquare = square;
                square.IsSelected = true;
                HighlightPossibleMoves(square);
            }
        }
        private void HighlightPossibleMoves(BoardSquare square)
        {
            var possibleMoves = CalculatePossibleMoves(square);
            foreach (var move in possibleMoves)
            {
                move.IsHighlighted = true;  // Presupunem că există o proprietate IsHighlighted în BoardSquare
            }
        }

        private List<BoardSquare> CalculatePossibleMoves(BoardSquare square)
        {
            var moves = new List<BoardSquare>();
            int[] directionRow = { 1, -1 }; // Piesele obișnuite se pot mișca doar înainte, regele în ambele direcții
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
                        // Adăugați logica pentru capturarea pieselor adversare
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
                square.IsHighlighted = false;  // Resetează evidențierea
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
                // Adăugați orice altă logică necesară după mișcare
            }
        }

        private bool IsValidMove(BoardSquare fromSquare, BoardSquare toSquare)
        {
            if (fromSquare.Checker == null || toSquare.Checker != null) return false;

            int rowDiff = toSquare.Row - fromSquare.Row;
            int colDiff = Math.Abs(toSquare.Column - fromSquare.Column);

            // Pentru piesele obișnuite, permite mutarea doar înainte (sau înapoi pentru rege)
            return colDiff == 1 && ((fromSquare.Checker.IsKing && Math.Abs(rowDiff) == 1) ||
                                    (fromSquare.Checker.Color == CheckerColor.White && rowDiff == -1) ||
                                    (fromSquare.Checker.Color == CheckerColor.Red && rowDiff == 1));
        }

        private void SetupBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    var isWhiteSquare = (row + column) % 2 == 0;
                    var checker = (row < 3 && !isWhiteSquare) ? new Checker { Color = CheckerColor.White } :
                                  (row > 4 && !isWhiteSquare) ? new Checker { Color = CheckerColor.Red } :
                                  null;

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
