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
        private readonly BoardViewModel boardViewModel;
        public PlayWindow(bool allowMultipleJumps, SettingsViewModel settingsViewModel)
        {
            InitializeComponent();
            boardViewModel = new BoardViewModel(allowMultipleJumps, settingsViewModel);
            DataContext = boardViewModel;
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
