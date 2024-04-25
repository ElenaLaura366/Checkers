using System;
using System.ComponentModel;
using System.Windows.Input;
using Checkers.Models;

namespace Checkers.ViewModels
{
    public class BoardSquare : INotifyPropertyChanged
    {
        private Checker checker;
        public bool IsWhiteSquare { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public ICommand SquareClickCommand { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        private bool isHighlighted;
        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (isHighlighted != value)
                {
                    isHighlighted = value;
                    OnPropertyChanged(nameof(IsHighlighted));
                }
            }
        }
        public BoardSquare(ICommand clickCommand)
        {
            SquareClickCommand = clickCommand ?? throw new ArgumentNullException(nameof(clickCommand));
        }
        public Checker Checker
        {
            get => checker;
            set
            {
                checker = value;
                OnPropertyChanged(nameof(Checker));
            }
        }
        public BoardSquare Clone()
        {
            var clonedSquare = new BoardSquare(this.SquareClickCommand)
            {
                Checker = this.Checker?.Clone(),
                IsHighlighted = this.IsHighlighted,
                IsSelected = this.IsSelected,
                IsWhiteSquare = this.IsWhiteSquare,
                Row = this.Row,
                Column = this.Column,
            };

            return clonedSquare;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
