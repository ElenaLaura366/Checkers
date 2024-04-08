using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Checkers
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand SettingsCommand { get; }
        public ICommand AboutCommand { get; }

        public MainViewModel()
        {
            SettingsCommand = new RelayCommand(settings);
            AboutCommand = new RelayCommand(about);
        }

        private void CloseMainWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                }
            }
        }

        void settings()
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
            CloseMainWindow();
        }

        void about()
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Show();
            CloseMainWindow();
        }
    }
}
