using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Anne.Foundation.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public Visibility WhiteSpace { get; set; }
        public Visibility NoWhiteSpace { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s == null)
                return WhiteSpace;

            return string.IsNullOrWhiteSpace(s) ? WhiteSpace : NoWhiteSpace;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}