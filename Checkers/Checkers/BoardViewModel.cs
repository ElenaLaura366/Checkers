using Microsoft.Win32;
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
using System.Xml;
using Newtonsoft.Json;

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
        public ICommand SaveCommand { get; private set; }
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
        public BoardViewModel(bool allowMultipleJumps, SettingsViewModel settings, GameState gameState = null)
        {
            AllowMultipleJumps = allowMultipleJumps;
            this.settingsViewModel = settings;
            Squares = new ObservableCollection<BoardSquare>();
            if (gameState != null)
            {
                LoadFromGameState(gameState);
            }
            else
            {
                SetupBoard();
            }
            UpdatePiecesCount();
            InitializeCommands();
            ActivePlayer = gameState?.ActivePlayer ?? CheckerColor.Red;
            SaveCommand = new RelayCommand(SaveGame);
        }
        private void LoadFromGameState(GameState gameState)
        {
            Squares.Clear();  // Clear any existing squares before loading new ones

            foreach (var squareState in gameState.BoardState)
            {
                var checker = squareState.CheckerColor.HasValue ? new Checker
                {
                    Color = (CheckerColor)squareState.CheckerColor,
                    IsKing = squareState.IsKing ?? false
                } : null;

                var square = new BoardSquare(new RelayCommand(param => HandleSquareSelected(param as BoardSquare)))
                {
                    Row = squareState.Row,
                    Column = squareState.Column,
                    Checker = checker,
                    IsWhiteSquare = (squareState.Row + squareState.Column) % 2 == 0
                };

                Squares.Add(square);
            }

            ActivePlayer = (CheckerColor)gameState.ActivePlayer;
            WhitePiecesCount = gameState.WhitePiecesCount;
            RedPiecesCount = gameState.RedPiecesCount;
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
            if (settingsViewModel.ShowStatistics)
            {
                var stats = GetGameStats();
                var message = $"{winner} wins the game!\n\nStatistics:\n" +
                    $"White Wins: {stats.WhiteWins}\n" +
                    $"Red Wins: {stats.RedWins}\n" +
                    $"Max Pieces Left for White: {stats.MaxWhitePiecesLeft}\n" +
                    $"Max Pieces Left for Red: {stats.MaxRedPiecesLeft}";
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
            int maxWhitePiecesLeft = 0;
            int maxRedPiecesLeft = 0;

            // Citirea datelor existente
            if (File.Exists(statsFilePath))
            {
                var lines = File.ReadAllLines(statsFilePath);
                if (lines.Length >= 4)
                {
                    int.TryParse(lines[0], out whiteWins);
                    int.TryParse(lines[1], out redWins);
                    int.TryParse(lines[2], out maxWhitePiecesLeft);
                    int.TryParse(lines[3], out maxRedPiecesLeft);
                }
            }

            // Actualizarea statisticii și a maximului de piese rămase
            if (winner == CheckerColor.White)
            {
                whiteWins++;
                if (remainingPieces > maxWhitePiecesLeft)
                {
                    maxWhitePiecesLeft = remainingPieces;
                }
            }
            else if (winner == CheckerColor.Red)
            {
                redWins++;
                if (remainingPieces > maxRedPiecesLeft)
                {
                    maxRedPiecesLeft = remainingPieces;
                }
            }

            // Scrierea datelor actualizate
            File.WriteAllLines(statsFilePath, new[]
            {
                whiteWins.ToString(),
                redWins.ToString(),
                maxWhitePiecesLeft.ToString(),
                maxRedPiecesLeft.ToString()
            });
        }

        public (int WhiteWins, int RedWins, int MaxWhitePiecesLeft, int MaxRedPiecesLeft) GetGameStats()
        {
            int whiteWins = 0;
            int redWins = 0;
            int maxWhitePiecesLeft = 0;
            int maxRedPiecesLeft = 0;

            if (File.Exists(statsFilePath))
            {
                var lines = File.ReadAllLines(statsFilePath);
                if (lines.Length >= 4)
                {
                    int.TryParse(lines[0], out whiteWins);
                    int.TryParse(lines[1], out redWins);
                    int.TryParse(lines[2], out maxWhitePiecesLeft);
                    int.TryParse(lines[3], out maxRedPiecesLeft);
                }
            }

            return (whiteWins, redWins, maxWhitePiecesLeft, maxRedPiecesLeft);
        }
        public void SaveGame()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json"; // Filtrează doar fișierele .json
            saveFileDialog.DefaultExt = "json"; // Setează extensia implicită ca .json
            saveFileDialog.AddExtension = true; // Asigură-te că extensia .json este adăugată

            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                var gameState = new
                {
                    ActivePlayer = this.ActivePlayer,
                    WhitePiecesCount = this.WhitePiecesCount,
                    RedPiecesCount = this.RedPiecesCount,
                    AllowMultipleJumps = this.AllowMultipleJumps,
                    BoardState = this.Squares.Select(s => new { s.Row, s.Column, CheckerColor = s.Checker?.Color, IsKing = s.Checker?.IsKing }).ToList()
                };

                string json = JsonConvert.SerializeObject(gameState, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(saveFileDialog.FileName, json);
            }
            else
            {
                MessageBox.Show("Saving canceled", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
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
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
