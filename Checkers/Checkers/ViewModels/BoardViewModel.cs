using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using Checkers.Models;

namespace Checkers.ViewModels
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BoardSquare> Squares { get; set; }
        private int whitePiecesCount;
        private int redPiecesCount;
        private BoardSquare selectedSquare;
        private CheckerColor activePlayer;
        private bool isMultipleJumpInProgress;
        private readonly bool allowMultipleJumps;
        private readonly SettingsViewModel settingsViewModel;
        private const string statsFilePath = "game_stats.txt";
        public bool AllowMultipleJumps => allowMultipleJumps;
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
            this.allowMultipleJumps = allowMultipleJumps;
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
        private void InitializeCommands()
        {
            foreach (var square in Squares)
            {
                BoardSquare currentSquare = square;
                square.SquareClickCommand = new RelayCommand(param => HandleSquareSelected(currentSquare));
            }
        }
        private void LoadFromGameState(GameState gameState)
        {
            Squares.Clear();

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
        private void SetupBoard()
        {
            ICommand squareCommand = new RelayCommand(param => HandleSquareSelected(param as BoardSquare));
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    var isWhiteSquare = (row + column) % 2 == 0;
                    var checker = (row < 3 && !isWhiteSquare) ? new Checker { Color = CheckerColor.White } :
                                  (row > 4 && !isWhiteSquare) ? new Checker { Color = CheckerColor.Red } :
                                  null;

                    var square = new BoardSquare(squareCommand)
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

        private BoardSquare GetSquare(int row, int column)
        {
            if (row >= 0 && row < 8 && column >= 0 && column < 8)
            {
                return Squares.FirstOrDefault(s => s.Row == row && s.Column == column);
            }
            return null;
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
        private IEnumerable<(int Row, int Column)> GetMoveDirections(Checker checker)
        {
            if (checker == null) yield break;

            if (checker.Color == CheckerColor.Red || checker.IsKing)
            {
                yield return (-1, -1);
                yield return (-1, 1);
            }

            if (checker.Color == CheckerColor.White || checker.IsKing)
            {
                yield return (1, -1);
                yield return (1, 1);
            }
        }
        private void UpdatePiecesCount()
        {
            WhitePiecesCount = Squares.Count(s => s.Checker?.Color == CheckerColor.White);
            RedPiecesCount = Squares.Count(s => s.Checker?.Color == CheckerColor.Red);
        }
        public void HandleSquareSelected(BoardSquare square)
        {
            if (activePlayer == CheckerColor.White)
            {
                Ai_move();
                ActivePlayer = CheckerColor.Red;
            }
            else
            {
                if (square.Checker?.Color == CheckerColor.Red)
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
        }
        private void HighlightPossibleMoves(BoardSquare square)
        {
            ClearSelections();

            var jumps = CalculateAllJumps(square, new List<BoardSquare>());
            var moves = CalculatePossibleMoves(square);

            var allMoves = jumps.Concat(moves).Distinct().ToList();

            foreach (var move in allMoves)
            {
                move.IsHighlighted = true;
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
                if (!path.Contains(jump))
                {
                    var nextPath = new List<BoardSquare>(path) { jump };
                    jumps.Add(jump);
                    jumps.AddRange(CalculateAllJumps(jump, nextPath));
                }
            }

            return jumps.Distinct().ToList();
        }
        private void CheckForPromotion(BoardSquare square)
        {
            if (square.Checker != null &&
                ((square.Row == 7 && square.Checker.Color == CheckerColor.White) ||
                 (square.Row == 0 && square.Checker.Color == CheckerColor.Red)))
            {
                square.Checker.IsKing = true;
                OnPropertyChanged(nameof(Squares));
            }
        }
        private List<BoardSquare> CalculatePossibleMoves(BoardSquare square)
        {
            var moves = CalculateImmediateJumps(square);
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
        private void MoveChecker(BoardSquare fromSquare, BoardSquare toSquare)
        {
            toSquare.Checker = fromSquare.Checker;
            fromSquare.Checker = null;

            int rowDiff = toSquare.Row - fromSquare.Row;
            if (Math.Abs(rowDiff) == 2)
            {
                PerformCapture(fromSquare, toSquare);
                CheckForPromotion(toSquare);
                UpdatePiecesCount();

                var additionalJumps = CalculatePossibleMoves(toSquare).Where(m => Math.Abs(m.Row - toSquare.Row) == 2).ToList();
                if (AllowMultipleJumps && additionalJumps.Any())
                {
                    isMultipleJumpInProgress = true;
                    selectedSquare = toSquare;
                    toSquare.IsSelected = true;
                    HighlightPossibleMoves(toSquare);
                }
                else
                {
                    isMultipleJumpInProgress = false;
                    selectedSquare = null;
                    ClearSelections();
                    ActivePlayer = ActivePlayer == CheckerColor.Red ? CheckerColor.White : CheckerColor.Red;
                }
            }
            else
            {
                CheckForPromotion(toSquare);
                ActivePlayer = ActivePlayer == CheckerColor.Red ? CheckerColor.White : CheckerColor.Red;
                isMultipleJumpInProgress = false;
                selectedSquare = null;
                ClearSelections();
            }
            var winner = CheckForWinner();
            if (winner != null)
            {
                EndGame(winner.Value);
            }
        }
        private void ManageMultiJump(BoardSquare toSquare)
        {
            var additionalJumps = CalculatePossibleMoves(toSquare).Where(m => Math.Abs(m.Row - toSquare.Row) == 2).ToList();
            if (AllowMultipleJumps && additionalJumps.Any())
            {
                isMultipleJumpInProgress = true;
                selectedSquare = toSquare;
                toSquare.IsSelected = true;
                HighlightPossibleMoves(toSquare);
            }
            else
            {
                isMultipleJumpInProgress = false;
                selectedSquare = null;
                ClearSelections();
                ActivePlayer = ActivePlayer == CheckerColor.Red ? CheckerColor.White : CheckerColor.Red;
            }
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

            return null;
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

            File.WriteAllLines(statsFilePath, new[]
            {
                whiteWins.ToString(),
                redWins.ToString(),
                maxWhitePiecesLeft.ToString(),
                maxRedPiecesLeft.ToString()
            });
        }
        private int CalculateRemainingPieces()
        {
            return Squares.Count(square => square.Checker != null);
        }
        public void SaveGame()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.DefaultExt = "json";
            saveFileDialog.AddExtension = true;

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
        int EvaluateGameState(BoardViewModel board)
        {
            int score = 0;
            foreach (var square in board.Squares)
            {
                if (square.Checker != null)
                {
                    if (square.Checker.Color == CheckerColor.White)
                    {
                        score += 10;
                        if (square.Checker.IsKing)
                            score += 20;
                    }
                    else
                    {
                        score -= 10;
                        if (square.Checker.IsKing)
                            score -= 20;
                    }
                }
            }
            return score;
        }
        public BoardViewModel Clone()
        {
            var clonedBoard = new BoardViewModel(this.allowMultipleJumps, this.settingsViewModel);
            ICommand clickCommand = new RelayCommand(param => HandleSquareSelected(param as BoardSquare));
            clonedBoard.Squares = new ObservableCollection<BoardSquare>(this.Squares.Select(square => new BoardSquare(clickCommand)
            {
                Row = square.Row,
                Column = square.Column,
                IsWhiteSquare = square.IsWhiteSquare,
                Checker = square.Checker == null ? null : new Checker
                {
                    Color = square.Checker.Color,
                    IsKing = square.Checker.IsKing
                }
            }));
            clonedBoard.activePlayer = this.activePlayer;
            clonedBoard.whitePiecesCount = this.whitePiecesCount;
            clonedBoard.redPiecesCount = this.redPiecesCount;
            return clonedBoard;
        }
        public void SimulateMove(BoardSquare fromSquare, BoardSquare toSquare, BoardViewModel simulatedBoard)
        {
            toSquare.Checker = fromSquare.Checker;
            fromSquare.Checker = null;
        }
        (int, BoardSquare, BoardSquare) Minimax(BoardViewModel board, int depth, bool maximizingPlayer, int alpha, int beta)
        {
            if (depth == 0 || board.CheckForWinner() != null)
            {
                return (EvaluateGameState(board), null, null);
            }

            BoardSquare bestSource = null;
            BoardSquare bestTarget = null;

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var square in board.Squares.Where(s => s.Checker?.Color == CheckerColor.White))
                {
                    var moves = board.CalculatePossibleMoves(square);
                    foreach (var target in moves)
                    {
                        var simulatedBoard = board.Clone();
                        simulatedBoard.SimulateMove(square, target, simulatedBoard);

                        var eval = Minimax(simulatedBoard, depth - 1, false, alpha, beta).Item1;

                        if (eval > maxEval)
                        {
                            maxEval = eval;
                            bestSource = square;
                            bestTarget = target;
                        }

                        alpha = Math.Max(alpha, eval);
                        if (beta <= alpha)
                            break;
                    }
                    if (beta <= alpha)
                        break;
                }
                return (maxEval, bestSource, bestTarget);
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var square in board.Squares.Where(s => s.Checker?.Color == CheckerColor.Red))
                {
                    var moves = board.CalculatePossibleMoves(square);
                    foreach (var target in moves)
                    {
                        var simulatedBoard = board.Clone();
                        simulatedBoard.SimulateMove(square, target, simulatedBoard);

                        var eval = Minimax(simulatedBoard, depth - 1, true, alpha, beta).Item1;

                        if (eval < minEval)
                        {
                            minEval = eval;
                            bestSource = square;
                            bestTarget = target;
                        }

                        beta = Math.Min(beta, eval);
                        if (beta <= alpha)
                            break;
                    }
                    if (beta <= alpha)
                        break;
                }
                return (minEval, bestSource, bestTarget);
            }
        }
        private void Ai_move()
        {
            if (ActivePlayer == CheckerColor.White)
            {
                var (score, source, target) = Minimax(this, 3, true, int.MinValue, int.MaxValue);
                if (source != null && target != null && source.Checker != null && target.Checker == null)
                {
                    MoveChecker(source, target);
                    ActivePlayer = CheckerColor.Red;
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
