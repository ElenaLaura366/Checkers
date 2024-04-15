using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Checkers.ViewModels
{
    public class AboutViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand BackCommand { get; }

        public AboutViewModel()
        {
            BackCommand = new RelayCommand(back);
        }

        void back()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            CloseAboutWindow();
        }

        private void CloseAboutWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is AboutWindow)
                {
                    window.Close();
                }
            }
        }
    }
}
