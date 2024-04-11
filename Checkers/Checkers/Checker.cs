using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
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

        public CheckerColor Color { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
