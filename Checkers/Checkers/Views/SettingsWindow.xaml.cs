using System;
using System.Windows;
using Checkers.ViewModels;

namespace Checkers
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
