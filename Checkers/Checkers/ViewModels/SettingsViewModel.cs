using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Checkers.Models;

namespace Checkers.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ICommand PlayCommand { get; }
        public ICommand OpenCommand { get; }
        private bool allowMultipleJumps;
        public bool AllowMultipleJumps
        {
            get => allowMultipleJumps;
            set
            {
                if (allowMultipleJumps != value)
                {
                    allowMultipleJumps = value;
                    OnPropertyChanged(nameof(AllowMultipleJumps));
                }
            }
        }
        private bool showStatistics;
        public bool ShowStatistics
        {
            get => showStatistics;
            set
            {
                if (showStatistics != value)
                {
                    showStatistics = value;
                    OnPropertyChanged(nameof(ShowStatistics));
                }
            }
        }
        private GameState loadedGameState;
        private bool gameLoaded;
        public bool CanChangeSettings => !GameLoaded;
        public bool GameLoaded
        {
            get => gameLoaded;
            set
            {
                gameLoaded = value;
                OnPropertyChanged(nameof(GameLoaded));
                OnPropertyChanged(nameof(CanChangeSettings));
            }
        }
        public SettingsViewModel()
        {
            PlayCommand = new RelayCommand(play);
            OpenCommand = new RelayCommand(open);
        }
        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is SettingsWindow)
                {
                    window.Close();
                }
            }
        }
        void play()
        {
            PlayWindow playWindow;
            if (GameLoaded)
            {
                playWindow = new PlayWindow(AllowMultipleJumps, this, loadedGameState);
            }
            else
            {
                playWindow = new PlayWindow(AllowMultipleJumps, this);
            }
            playWindow.Show();
            CloseWindow();
        }
        void open()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    loadedGameState = JsonConvert.DeserializeObject<GameState>(fileContent);
                    LoadGame(loadedGameState);
                    GameLoaded = true;
                    MessageBox.Show("Game loaded successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load the game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoadGame(GameState gameState)
        {
            AllowMultipleJumps = gameState.AllowMultipleJumps;
            var playWindow = (PlayWindow)Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is PlayWindow);
            if (playWindow != null && gameState != null)
            {
                // Create a new BoardViewModel with the loaded game state
                var newViewModel = new BoardViewModel(gameState.AllowMultipleJumps, this)
                {
                    ActivePlayer = (CheckerColor)gameState.ActivePlayer,
                    WhitePiecesCount = gameState.WhitePiecesCount,
                    RedPiecesCount = gameState.RedPiecesCount
                };

                // Clear existing squares and load the new state
                newViewModel.Squares.Clear();
                foreach (var squareState in gameState.BoardState)
                {
                    var checker = squareState.CheckerColor.HasValue ? new Checker
                    {
                        Color = (CheckerColor)squareState.CheckerColor,
                        IsKing = squareState.IsKing ?? false
                    } : null;

                    var square = new BoardSquare(new RelayCommand(param => newViewModel.HandleSquareSelected(param as BoardSquare)))
                    {
                        Row = squareState.Row,
                        Column = squareState.Column,
                        Checker = checker,
                        IsWhiteSquare = (squareState.Row + squareState.Column) % 2 == 0
                    };

                    newViewModel.Squares.Add(square);
                }

                // Update the DataContext of the play window
                playWindow.DataContext = newViewModel;

                // This assumes PlayWindow has a method or mechanism to update the UI with the new BoardViewModel
                playWindow.RefreshBoard();
            }
        }
    }
}
