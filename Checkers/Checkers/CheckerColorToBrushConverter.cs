using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace Checkers
{
    public class CheckerColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CheckerColor checkerColor)
            {
                switch (checkerColor)
                {
                    case CheckerColor.White:
                        return Brushes.White;
                    case CheckerColor.Red:
                        return Brushes.Red;
                    default:
                        return DependencyProperty.UnsetValue; // Invalid color
                }
            }
            return DependencyProperty.UnsetValue; // Invalid input
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(); // This converter only converts one way
        }
    }
}
