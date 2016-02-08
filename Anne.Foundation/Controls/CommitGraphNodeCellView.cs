using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Anne.Foundation.CommitGraph;

namespace Anne.Foundation.Controls
{
    public class CommitGraphNodeCellView : Control
    {
        private static readonly Pen EdgePen;

        private static Pen _gridPen;

        static CommitGraphNodeCellView()
        {
            EdgePen = new Pen(Brushes.MediumVioletRed, 1);
            EdgePen.Freeze();
        }

        public CommitGraphNodeCellView()
        {
            ClipToBounds = true;
            DataContextChanged += (_, __) => InvalidateVisual();
        }

        private CommitGraphNode CommitGraphNode => DataContext as CommitGraphNode;

        protected override void OnRender(DrawingContext dc)
        {
            var cellWidth = Height;

            {
                const double radius = 4;
                var cx = Height*0.5 + CommitGraphNode.NodeIndex*cellWidth;
                var cy = Height*0.5;

                dc.DrawEllipse(Brushes.MistyRose, EdgePen, new Point(cx, cy), radius, radius);
            }

            // 確認用
            {
                // http://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html
                if (_gridPen == null)
                {
                    var presentationSource = PresentationSource.FromVisual(this);
                    if (presentationSource?.CompositionTarget != null)
                    {
                        var gridBrush = new SolidColorBrush(Color.FromArgb(0x30, 0x30, 0x30, 0x30));
                        gridBrush.Freeze();

                        var m = presentationSource.CompositionTarget.TransformToDevice;
                        _gridPen = new Pen(gridBrush, 1/m.M11); // 物理ピクセルで１
                        _gridPen.Freeze();
                    }

                    Debug.Assert(_gridPen != null);
                }

                const int lineCount = 20;
                var p0 = new Point(0, 0);
                var p1 = new Point(0, Height);

                using (new GuidelineSetBlock(
                    dc,
                    xGuides: Enumerable.Range(0, lineCount).Select(x => x*cellWidth + _gridPen.Thickness*0.5),
                    yGuides: new[] {0, Height}))
                {
                    for (var x = 0; x != lineCount; ++x)
                    {
                        p0.X = x*cellWidth;
                        p1.X = x*cellWidth;
                        dc.DrawLine(_gridPen, p0, p1);
                    }
                }
            }
        }
    }
}