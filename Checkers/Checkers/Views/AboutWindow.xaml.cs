using System;
using System.Windows;
using Checkers.ViewModels;

namespace Checkers
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = new AboutViewModel();
        }
    }
}
