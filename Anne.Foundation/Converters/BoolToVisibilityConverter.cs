using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Anne.Foundation.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility True { get; set; }
        public Visibility False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool == false)
                return True;

            return (bool) value ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}