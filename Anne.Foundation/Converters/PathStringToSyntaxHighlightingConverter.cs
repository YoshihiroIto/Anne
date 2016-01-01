using System;
using System.Globalization;
using System.Windows.Data;

namespace Anne.Foundation.Converters
{
    public class PathStringToSyntaxHighlightingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if true
            return null;
#else
            var path = value as string;
            if (path == null)
                return null;

            var ext = System.IO.Path.GetExtension(path);

            return HighlightingManager.Instance.GetDefinitionByExtension(ext);
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}