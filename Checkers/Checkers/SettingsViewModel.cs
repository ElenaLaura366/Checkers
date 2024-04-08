﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Checkers
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
            var playWindow = new PlayWindow();
            playWindow.Show();
            CloseWindow();
        }

        void open()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                MessageBox.Show("File opened: " + openFileDialog.FileName);
            }
        }
    }
}