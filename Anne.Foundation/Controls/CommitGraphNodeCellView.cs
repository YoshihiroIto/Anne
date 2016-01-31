using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Anne.Foundation.CommitGraph;

namespace Anne.Foundation.Controls
{
    public class CommitGraphNodeCellView : Control
    {
        private static readonly Pen EdgePen;

        static CommitGraphNodeCellView()
        {
            EdgePen = new Pen(Brushes.MediumVioletRed, 0.5);
            EdgePen.Freeze();
        }

        private CommitGraphNode CommitGraphNode => DataContext as CommitGraphNode;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var cellWidth = Height;

            const double radius = 4;
            var cx = Height*0.5 + CommitGraphNode.NodeIndex * cellWidth;
            var cy = Height*0.5;

            dc.DrawEllipse(Brushes.MistyRose, EdgePen, new Point(cx, cy), radius, radius);
        }
    }
}
