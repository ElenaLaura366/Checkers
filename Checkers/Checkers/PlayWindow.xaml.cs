using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        public void UpdateBoardViewModel(BoardViewModel viewModel)
        {
            boardViewModel = viewModel;
            DataContext = viewModel;
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
