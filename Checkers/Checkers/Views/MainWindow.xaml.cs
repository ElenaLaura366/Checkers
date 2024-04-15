using System;
using System.Windows;
using Checkers.ViewModels;

namespace Checkers
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

    }
}
