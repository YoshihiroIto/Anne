using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Anne.Foundation.Controls
{
    public class PathTextBlock : TextBlock
    {
        // http://www.k-brand.gr.jp/log/002168 を参考にしました

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        private static string TruncatePath(string path, int length)
        {
            var sb = new StringBuilder(length + 1);
            PathCompactPathEx(sb, path, length + 1, 0);
            return sb.ToString();
        }

        public string PathText
        {
            get { return (string) GetValue(PathTextProperty); }
            set { SetValue(PathTextProperty, value); }
        }

        public static readonly DependencyProperty PathTextProperty =
            DependencyProperty.Register(nameof(PathText), typeof (string), typeof (PathTextBlock),
                new UIPropertyMetadata(string.Empty, (d, e) =>
                {
                    var self = d as PathTextBlock;
                    Debug.Assert(self != null);

                    self.ToolTip = e.NewValue;

                    self.UpdateText(self.ActualWidth);
                }));

        public PathTextBlock()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += (s, e) => UpdateText(ActualWidth);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = Parent as FrameworkElement;
            if (parent != null)
                parent.SizeChanged += ParentOnSizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = Parent as FrameworkElement;
            if (parent != null)
                parent.SizeChanged -= ParentOnSizeChanged;
        }

        private void ParentOnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (args.WidthChanged)
                UpdateText(args.NewSize.Width);
        }

        private void UpdateText(double targetWidth)
        {
            targetWidth -= Padding.Left + Padding.Right;

            if (targetWidth <= 0.0)
                return;

            if (string.IsNullOrEmpty(PathText))
            {
                Text = string.Empty;
                return;
            }

            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

            var text = PathText;
            for (var length = text.Length; length != 0; --length)
            {
                var ft = new FormattedText(
                    text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, FontSize, Foreground);

                if (ft.WidthIncludingTrailingWhitespace <= targetWidth)
                    break;

                text = TruncatePath(PathText, length);
            }

            if (string.IsNullOrEmpty(text))
                text = "???";

            Text = text;
        }
    }
}