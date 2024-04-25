using System;
using System.ComponentModel;

namespace Checkers.Models
{
    public enum CheckerColor
    {
        None,
        White,
        Red
    }

    public class Checker : INotifyPropertyChanged
    {
        private bool isKing;
        public bool IsKing
        {
            get => isKing;
            set
            {
                if (isKing != value)
                {
                    isKing = value;
                    OnPropertyChanged(nameof(IsKing));
                }
            }
        }
        public CheckerColor Color { get; set; }

        public Checker Clone()
        {
            return new Checker
            {
                Color = this.Color,
                IsKing = this.IsKing
            };
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
