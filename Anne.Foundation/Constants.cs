using System.Windows.Media;

namespace Anne.Foundation
{
    public static class Constants
    {
        public static readonly SolidColorBrush ChunckTagBackground =
            new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xFF));

        public static readonly SolidColorBrush AddeBackground =
            new SolidColorBrush(Color.FromRgb(0xDD, 0xFF, 0xDD));

        public static readonly SolidColorBrush RemoveBackground =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xDD, 0xDD));

        static Constants()
        {
            ChunckTagBackground.Freeze();
            AddeBackground.Freeze();
            RemoveBackground.Freeze();
        }
    }
}