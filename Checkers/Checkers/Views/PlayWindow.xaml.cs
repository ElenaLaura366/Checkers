using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Checkers.ViewModels;
using Checkers.Models;

namespace Checkers
{
    public partial class PlayWindow : Window
    {
        internal BoardViewModel boardViewModel { get; private set; }
        public PlayWindow(bool allowMultipleJumps, SettingsViewModel settingsViewModel, GameState gameState = null)
        {
            InitializeComponent();
            if (gameState != null)
            {
                boardViewModel = new BoardViewModel(allowMultipleJumps, settingsViewModel, gameState);
            }
            else
            {
                boardViewModel = new BoardViewModel(allowMultipleJumps, settingsViewModel);
            }
            DataContext = boardViewModel;
        }
        public void RefreshBoard()
        {
            BoardItemsControl.ItemsSource = null;
            BoardItemsControl.ItemsSource = ((BoardViewModel)DataContext).Squares;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is BoardSquare square)
            {
                (DataContext as BoardViewModel)?.HandleSquareSelected(square);
            }
        }
    }
}
