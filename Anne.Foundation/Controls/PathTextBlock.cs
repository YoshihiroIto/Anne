using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Reactive.Linq;
using Anne.Foundation.Extentions;
using Jewelry.Collections;
using Reactive.Bindings.Extensions;

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

                    var parent = self.Parent as FrameworkElement;
                    if (parent != null)
                        self.UpdateText(parent.ActualWidth);
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

        // ReSharper disable once StringLiteralTypo
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        private static string TruncatePath(string path, int length, StringBuilder buffer)
        {
            buffer.Clear();
            PathCompactPathEx(buffer, path, length, 0);
            return buffer.ToString();
        }

        public PathTextBlock()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private IDisposable _parentSizeChangedObservable;

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = Parent as FrameworkElement;
            if (parent != null)
            {
                _parentSizeChangedObservable = parent.SizeChangedAsObservable()
                    .Where(x => x.WidthChanged)
                    .Throttle(TimeSpan.FromMilliseconds(1))
                    .ObserveOnUIDispatcher()
                    .Subscribe(x => UpdateText(x.NewSize.Width));

                UpdateText(parent.ActualWidth);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _parentSizeChangedObservable?.Dispose();
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

            var currentCulture = CultureInfo.CurrentCulture;
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var fontSize = FontSize;
            var foreground = Foreground;

            var pathText = PathText;
            var text = PathText;
            var textLength = text.Length;

            var buffer = new StringBuilder(textLength * 2 + 1);

            // 最低限入る長さまで求める
            while (textLength > 0)
            {
                var textWidth = TextWidthCache.GetOrAdd(
                    text,
                    t =>
                    {
                        var ft = new FormattedText(
                            t, currentCulture, FlowDirection.LeftToRight, typeface, fontSize, foreground);
                        return ft.WidthIncludingTrailingWhitespace;
                    });

                if (textWidth <= targetWidth)
                    break;

                textLength >>= 1;
                text = TruncatePath(pathText, textLength, buffer);
            }

            // ぎりぎりまで詰める
            if (textLength != pathText.Length)
            {
                for (var step = textLength; step > 0; step >>= 1)
                {
                    var nextTextLength = textLength + step;
                    var nextText = TruncatePath(pathText, nextTextLength, buffer);

                    var nextTextWidth = TextWidthCache.GetOrAdd(
                        nextText,
                        t =>
                        {
                            var ft = new FormattedText(
                                t, currentCulture, FlowDirection.LeftToRight, typeface, fontSize, foreground);
                            return ft.WidthIncludingTrailingWhitespace;
                        });

                    if (nextTextWidth <= targetWidth)
                        textLength += step;
                }

                text = TruncatePath(pathText, textLength, buffer);
            }

            UpdateInlines(text);
        }

        private static readonly LruCache<string, double> TextWidthCache = new LruCache<string, double>(10000, false);

        // ディレクトリ部とファイル部を分離し色分けする
        private void UpdateInlines(string text)
        {
            var isEmpty = string.IsNullOrEmpty(text);

            var dirname = isEmpty ? string.Empty : Path.GetDirectoryName(text) ?? string.Empty;
            var filename = isEmpty ? string.Empty : Path.GetFileName(text);

            if (dirname.Length > 0)
                dirname += @"\";

            if (Inlines.Count != 2)
            {
                Inlines.Clear();
                Inlines.Add(new Run(dirname) {Foreground = DirnameForeground});
                Inlines.Add(new Run(filename) {Foreground = FilenameForeground});
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