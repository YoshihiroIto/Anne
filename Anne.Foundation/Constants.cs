using System.Windows.Media;

namespace Anne.Foundation
{
    public static class Constants
    {
        public static readonly SolidColorBrush ChunckTagBackground =
            new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xFF));

        public static readonly SolidColorBrush AddeBackground =
            new SolidColorBrush(Color.FromRgb(0xDD, 0xFF, 0xDD));

        public static readonly SolidColorBrush RemoveBackground =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xDD, 0xDD));

        public static readonly Pen FramePen = new Pen(Brushes.LightGray, 0.5);

        static Constants()
        {
            ChunckTagBackground.Freeze();
            AddeBackground.Freeze();
            RemoveBackground.Freeze();
            FramePen.Freeze();
        }
    }
}