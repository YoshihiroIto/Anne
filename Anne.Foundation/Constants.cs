using System.Windows.Media;

namespace Anne.Foundation
{
    public static class Constants
    {
        public static readonly Pen FramePen = new Pen(Brushes.LightGray, 0.5);

        public static readonly SolidColorBrush ChunckTagBackground =
            new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xFF));

        public static readonly SolidColorBrush AddBackground =
            new SolidColorBrush(Color.FromRgb(0xDD, 0xFF, 0xDD));

        public static readonly SolidColorBrush RemoveBackground =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xDD, 0xDD));

        public static readonly SolidColorBrush LightChunckTagBackground =
            new SolidColorBrush(Color.FromRgb(0xF8, 0xF8, 0xFF));

        public static readonly SolidColorBrush LightAddBackground =
            new SolidColorBrush(Color.FromRgb(0xEE, 0xFF, 0xEE));

        public static readonly SolidColorBrush LightRemoveBackground =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xEE, 0xEE));

        public static readonly SolidColorBrush HighlightAddBackground =
            new SolidColorBrush(Color.FromRgb(0xBB, 0xFF, 0xBB));

        public static readonly SolidColorBrush HighlightRemoveBackground =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xBB, 0xBB));

        static Constants()
        {
            FramePen.Freeze();

            ChunckTagBackground.Freeze();
            AddBackground.Freeze();
            RemoveBackground.Freeze();

            LightChunckTagBackground.Freeze();
            LightAddBackground.Freeze();
            LightRemoveBackground.Freeze();

            HighlightAddBackground.Freeze();
            HighlightRemoveBackground.Freeze();
        }
    }
}