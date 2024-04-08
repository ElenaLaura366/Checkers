using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkers
{
    public class BoardSquare : INotifyPropertyChanged
    {
        private Checker checker;
        public bool IsWhiteSquare { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public ICommand SquareClickCommand { get; set; }

        public Checker Checker
        {
            get => checker;
            set
            {
                checker = value;
                OnPropertyChanged(nameof(Checker));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
