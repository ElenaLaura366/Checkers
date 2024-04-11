using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public bool AllowMultipleJumps { get; }
        private SettingsViewModel settingsViewModel;
        private bool isMultipleJumpInProgress;
        private const string statsFilePath = "game_stats.txt";

        public CheckerColor ActivePlayer
        {
            get => activePlayer;
            set
            {
                if (activePlayer != value)
                {
                    activePlayer = value;
                    OnPropertyChanged(nameof(ActivePlayer));
                }
            }
        }
        private CheckerColor activePlayer;

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

        public BoardViewModel(bool allowMultipleJumps, SettingsViewModel settings)
        {
            AllowMultipleJumps = allowMultipleJumps;
            this.settingsViewModel = settings;
            Squares = new ObservableCollection<BoardSquare>();
            SetupBoard();
            UpdatePiecesCount();
            InitializeCommands();
            ActivePlayer = CheckerColor.Red;
        }

        private void UpdatePiecesCount()
        {
            WhitePiecesCount = Squares.Count(s => s.Checker?.Color == CheckerColor.White);
            RedPiecesCount = Squares.Count(s => s.Checker?.Color == CheckerColor.Red);
        }

        public void HandleSquareSelected(BoardSquare square)
        {
            if (square.Checker?.Color == ActivePlayer)
            {
                if (!isMultipleJumpInProgress || (isMultipleJumpInProgress && square == selectedSquare))
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
                    else if (square.Checker != null)
                    {
                        ClearSelections();
                        selectedSquare = square;
                        square.IsSelected = true;
                        HighlightPossibleMoves(square);
                    }
                }
            }
            else if (square.IsHighlighted && selectedSquare != null)
            {
                MoveChecker(selectedSquare, square);
                ClearSelections();
            }
        }
        private List<BoardSquare> CalculateImmediateJumps(BoardSquare square)
        {
            var moves = new List<BoardSquare>();
            var directions = GetMoveDirections(square.Checker);

            foreach (var direction in directions)
            {
                BoardSquare jumpSquare = GetSquare(square.Row + 2 * direction.Row, square.Column + 2 * direction.Column);
                BoardSquare overSquare = GetSquare(square.Row + direction.Row, square.Column + direction.Column);

                if (jumpSquare != null && overSquare != null && overSquare.Checker != null &&
                    overSquare.Checker.Color != square.Checker.Color && jumpSquare.Checker == null)
                {
                    moves.Add(jumpSquare);
                }
            }

            return moves;
        }
        private List<BoardSquare> CalculateAllJumps(BoardSquare square, List<BoardSquare> path)
        {
            var jumps = new List<BoardSquare>();
            var immediateJumps = CalculateImmediateJumps(square);

            foreach (var jump in immediateJumps)
            {
                if (!path.Contains(jump)) // Evită ciclarea
                {
                    var nextPath = new List<BoardSquare>(path) { jump };
                    jumps.Add(jump);
                    jumps.AddRange(CalculateAllJumps(jump, nextPath));
                }
            }

            return jumps.Distinct().ToList();
        }

        private void HighlightPossibleMoves(BoardSquare square)
        {
            ClearSelections();

            var moves = CalculateAllJumps(square, new List<BoardSquare>());
            if (!moves.Any()) // Dacă nu există sărituri, afișează mișcările normale
            {
                moves = CalculatePossibleMoves(square);
            }

            foreach (var move in moves)
            {
                move.IsHighlighted = true;
            }
        }
        private IEnumerable<(int Row, int Column)> GetMoveDirections(Checker checker)
        {
            if (checker == null) yield break;

            if (checker.Color == CheckerColor.Red || checker.IsKing)
            {
                yield return (-1, -1); // sus-stânga pentru roșu sau rege
                yield return (-1, 1);  // sus-dreapta pentru roșu sau rege
            }

            if (checker.Color == CheckerColor.White || checker.IsKing)
            {
                yield return (1, -1);  // jos-stânga pentru alb sau rege
                yield return (1, 1);   // jos-dreapta pentru alb sau rege
            }
        }
        private BoardSquare GetSquare(int row, int column)
        {
            if (row >= 0 && row < 8 && column >= 0 && column < 8)
            {
                return Squares.FirstOrDefault(s => s.Row == row && s.Column == column);
            }
            return null;
        }
        private void CheckForPromotion(BoardSquare square)
        {
            if (square.Checker != null &&
                ((square.Row == 7 && square.Checker.Color == CheckerColor.White) ||
                 (square.Row == 0 && square.Checker.Color == CheckerColor.Red)))
            {
                square.Checker.IsKing = true;  // Aceasta ar trebui să declanșeze notificarea
                OnPropertyChanged(nameof(Squares));
            }
        }
        private List<BoardSquare> CalculatePossibleMoves(BoardSquare square)
        {
            var moves = CalculateImmediateJumps(square);
            if (moves.Any())
            {
                return moves;
            }

            // Adaugă mișcările normale dacă nu există sărituri
            var directions = GetMoveDirections(square.Checker);
            foreach (var direction in directions)
            {
                BoardSquare targetSquare = GetSquare(square.Row + direction.Row, square.Column + direction.Column);
                if (targetSquare != null && targetSquare.Checker == null)
                {
                    moves.Add(targetSquare);
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
        private void PerformCapture(BoardSquare fromSquare, BoardSquare toSquare)
        {
            int middleRow = (fromSquare.Row + toSquare.Row) / 2;
            int middleColumn = (fromSquare.Column + toSquare.Column) / 2;
            BoardSquare capturedSquare = Squares.FirstOrDefault(s => s.Row == middleRow && s.Column == middleColumn);

            if (capturedSquare != null)
            {
                capturedSquare.Checker = null;
            }
        }
        private void MoveChecker(BoardSquare fromSquare, BoardSquare toSquare)
        {
            toSquare.Checker = fromSquare.Checker;
            fromSquare.Checker = null;

            int rowDiff = toSquare.Row - fromSquare.Row;
            if (Math.Abs(rowDiff) == 2) // Dacă este o captură
            {
                PerformCapture(fromSquare, toSquare);
                CheckForPromotion(toSquare);
                UpdatePiecesCount();

                var additionalJumps = CalculatePossibleMoves(toSquare).Where(m => Math.Abs(m.Row - toSquare.Row) == 2).ToList();
                if (AllowMultipleJumps && additionalJumps.Any())
                {
                    // Continuă săriturile multiple dacă sunt posibile
                    isMultipleJumpInProgress = true;
                    selectedSquare = toSquare;
                    toSquare.IsSelected = true;
                    HighlightPossibleMoves(toSquare);
                }
                else
                {
                    // Termină săriturile multiple și schimbă jucătorul
                    isMultipleJumpInProgress = false;
                    selectedSquare = null;
                    ClearSelections();
                    ActivePlayer = ActivePlayer == CheckerColor.Red ? CheckerColor.White : CheckerColor.Red;
                }
            }
            else
            {
                // Pentru mișcările normale, schimbă jucătorul activ
                CheckForPromotion(toSquare);
                ActivePlayer = ActivePlayer == CheckerColor.Red ? CheckerColor.White : CheckerColor.Red;
                isMultipleJumpInProgress = false;
                selectedSquare = null;
                ClearSelections();
            }

            // Verifică dacă există un câștigător
            var winner = CheckForWinner();
            if (winner != null)
            {
                EndGame(winner.Value);
            }
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

            bool isKing = fromSquare.Checker.IsKing;
            bool isForwardMove = (fromSquare.Checker.Color == CheckerColor.White && rowDiff == -1) ||
                                 (fromSquare.Checker.Color == CheckerColor.Red && rowDiff == 1);

            return colDiff == 1 && (isForwardMove || (isKing && Math.Abs(rowDiff) == 1));
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

        public CheckerColor? CheckForWinner()
        {
            if (WhitePiecesCount == 0 || !HasValidMoves(CheckerColor.White))
            {
                return CheckerColor.Red;
            }
            if (RedPiecesCount == 0 || !HasValidMoves(CheckerColor.Red))
            {
                return CheckerColor.White;
            }

            return null; // Jocul continuă
        }

        private bool HasValidMoves(CheckerColor playerColor)
        {
            foreach (var square in Squares.Where(s => s.Checker?.Color == playerColor))
            {
                if (CalculatePossibleMoves(square).Any())
                {
                    return true;
                }
            }
            return false;
        }
        private int CalculateRemainingPieces()
        {
            return Squares.Count(square => square.Checker != null);
        }

        private void EndGame(CheckerColor winner)
        {
            int remainingPieces = CalculateRemainingPieces();
            UpdateStatsFile(winner, remainingPieces);
            if (settingsViewModel.ShowStatistics)  // Asigură-te că ai acces la instanța corectă a SettingsViewModel
            {
                var stats = GetGameStats();
                var message = $"{winner} wins the game!\n\nStatistics:\nWhite Wins: {stats.WhiteWins}\nRed Wins: {stats.RedWins}\nPieces Left for last winner: {stats.LastWinnerPiecesLeft}";
                MessageBox.Show(message);
            }
            else
            {
                MessageBox.Show($"{winner} wins the game!");
            }
            var mainWindow = new MainWindow();
            mainWindow.Show();
            foreach (Window window in Application.Current.Windows)
            {
                if (window is PlayWindow)
                {
                    window.Close();
                }
            }
        }
        private void UpdateStatsFile(CheckerColor winner, int remainingPieces)
        {
            int whiteWins = 0;
            int redWins = 0;
            // Nu mai avem nevoie de maxPiecesLeft pentru a stoca maximul global

            // Citirea datelor existente
            if (File.Exists(statsFilePath))
            {
                var lines = File.ReadAllLines(statsFilePath);
                if (lines.Length >= 2)
                {
                    int.TryParse(lines[0], out whiteWins);
                    int.TryParse(lines[1], out redWins);
                    // Nu mai citim maxPiecesLeft
                }
            }

            // Actualizarea statisticii
            if (winner == CheckerColor.White)
            {
                whiteWins++;
            }
            else if (winner == CheckerColor.Red)
            {
                redWins++;
            }

            // Scrierea datelor actuale, inclusiv numărul de piese rămase pentru câștigătorul acestui joc
            File.WriteAllLines(statsFilePath, new[]
            {
                whiteWins.ToString(),
                redWins.ToString(),
                remainingPieces.ToString()  // Aici salvăm numărul de piese rămase pentru câștigătorul ultimului joc
            });
        }
        public (int WhiteWins, int RedWins, int LastWinnerPiecesLeft) GetGameStats()
        {
            int whiteWins = 0;
            int redWins = 0;
            int lastWinnerPiecesLeft = 0;

            if (File.Exists(statsFilePath))
            {
                var lines = File.ReadAllLines(statsFilePath);
                if (lines.Length >= 3)
                {
                    int.TryParse(lines[0], out whiteWins);
                    int.TryParse(lines[1], out redWins);
                    int.TryParse(lines[2], out lastWinnerPiecesLeft);  // Aceasta va fi numărul de piese rămase pentru ultimul câștigător
                }
            }

            return (whiteWins, redWins, lastWinnerPiecesLeft);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
