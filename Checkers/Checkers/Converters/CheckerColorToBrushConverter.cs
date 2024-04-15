using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using Checkers.Models;

namespace Checkers.Converters
{
    public class CheckerColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Checker checker)
            {
                if (checker.IsKing)
                {
                    return checker.Color == CheckerColor.White ? Brushes.Black : Brushes.Green;
                }
                else
                {
                    switch (checker.Color)
                    {
                        case CheckerColor.White:
                            return Brushes.White;
                        case CheckerColor.Red:
                            return Brushes.Red;
                        default:
                            return DependencyProperty.UnsetValue;
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
