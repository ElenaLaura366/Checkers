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
    /// <summary>
    /// Interaction logic for PlayWindow.xaml
    /// </summary>
    public partial class PlayWindow : Window
    {
        public PlayWindow()
        {
            InitializeComponent();
            DataContext = new BoardViewModel();
        }
        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ellipse = sender as Ellipse;
            if (ellipse != null)
            {
                // Obțineți patratul asociat elipsei
                var square = ellipse.DataContext as BoardSquare;
                if (square != null)
                {
                    // Apelati metoda HandleSquareSelected din BoardViewModel
                    (DataContext as BoardViewModel)?.HandleSquareSelected(square);
                }
            }
        }

    }
}
