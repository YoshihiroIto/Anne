using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Anne.Foundation.Controls
{
    public class PathTextBlock : TextBlock
    {
        #region PathText

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

        #endregion

        #region DirnameForeground

        public Brush DirnameForeground
        {
            get { return (Brush) GetValue(DirnameForegroundProperty); }
            set { SetValue(DirnameForegroundProperty, value); }
        }

        public static readonly DependencyProperty DirnameForegroundProperty =
            DependencyProperty.Register(
                "DirnameForeground",
                typeof (Brush),
                typeof (PathTextBlock),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = Brushes.LightSlateGray,
                    BindsTwoWayByDefault = true
                });

        #endregion

        #region FilenameForeground

        public Brush FilenameForeground
        {
            get { return (Brush) GetValue(FilenameForegroundProperty); }
            set { SetValue(FilenameForegroundProperty, value); }
        }

        public static readonly DependencyProperty FilenameForegroundProperty =
            DependencyProperty.Register(
                "FilenameForeground",
                typeof (Brush),
                typeof (PathTextBlock),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = Brushes.Black,
                    BindsTwoWayByDefault = true
                });

        #endregion

        // http://www.k-brand.gr.jp/log/002168 を参考にしました

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        private static string TruncatePath(string path, int length)
        {
            var sb = new StringBuilder(length + 1);
            PathCompactPathEx(sb, path, length + 1, 0);
            return sb.ToString();
        }

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
            {
                Inlines.Clear();
                Text = "???";
            }

            // ディレクトリ部とファイル部を分離し色分けする
            {
                var dirname = Path.GetDirectoryName(text) ?? string.Empty;
                var filename = Path.GetFileName(text);

                if (dirname.Length > 0)
                    dirname += @"\";

                if (Inlines.Count != 2)
                {
                    Inlines.Clear();
                    Inlines.Add(new Run(dirname) { Foreground = DirnameForeground });
                    Inlines.Add(new Run(filename) { Foreground = FilenameForeground });
                }
                else
                {
                    var first = Inlines.FirstInline as Run;
                    var last = Inlines.LastInline as Run;

                    Debug.Assert(first != null);
                    Debug.Assert(last != null);

                    if (first.Text != dirname)
                        first.Text = dirname;

                    if (last.Text != filename)
                        last.Text = filename;
                }
            }
        }
    }
}