using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Anne.Features.Interfaces;
using Anne.Foundation;
using Anne.Foundation.Controls;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace Anne.Diff
{
    public class DiffTextEditorLeftMargin : AbstractMargin
    {
        private readonly DiffTextEditor _editor;
        private DiffLine[] DiffLines => ((IFileDiffVm) _editor?.DataContext)?.DiffLines.Value;

        private const double IndexWidth = 40;
        private const double LineTypeWidth = 16;

        private const double MarginWidth = IndexWidth + IndexWidth + LineTypeWidth;
        private const double OldIndexOffset = 0;
        private const double NewIndexOffset = IndexWidth;
        private const double LineTypeIndexOffset = IndexWidth + IndexWidth;

        public DiffTextEditorLeftMargin(DiffTextEditor editor)
        {
            Debug.Assert(editor != null);

            _editor = editor;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(MarginWidth, 0);
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= VisualLinesChanged;
                oldTextView.ScrollOffsetChanged -= ScrollOffsetChanged;
            }

            base.OnTextViewChanged(oldTextView, newTextView);

            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += VisualLinesChanged;
                newTextView.ScrollOffsetChanged += ScrollOffsetChanged;
            }
        }

        private void VisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void ScrollOffsetChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            const double x0 = 0;
            const double w0 = MarginWidth - LineTypeWidth;
            DiffTextEditorHelper.DrawBackground(TextView, dc, x0, w0, DiffLines, false, DrawForegroundLeft);

            const double x1 = IndexWidth*2;
            const double w1 = LineTypeWidth;
            DiffTextEditorHelper.DrawBackground(TextView, dc, x1, w1, DiffLines, true, DrawForeground);
        }

        private static void DrawForegroundLeft(DiffTextEditorHelper.PerLineDrawArgs args)
        {
            if (args.Index == 0)
                DrawIndex(args.DrawingContext, args.Rect, args.DiffLine);
        }

        private static Pen _gridPen;

        private static void DrawForeground(DiffTextEditorHelper.PerLineDrawArgs args)
        {
            if (_gridPen == null)
            {
                var presentationSource = PresentationSource.FromVisual(args.TextView);
                if (presentationSource?.CompositionTarget != null)
                {
                    var m = presentationSource.CompositionTarget.TransformToDevice;
                    _gridPen = new Pen(Constants.FrameBrush, 1 / m.M11);   // 物理ピクセルで１
                    _gridPen.Freeze();
                }

                Debug.Assert(_gridPen != null);
            }

            var halfPenWidth = _gridPen.Thickness * 0.5;

            using (new GuidelineSetBlock(
                args.DrawingContext,
                xGuides: new[]
                {
                    NewIndexOffset + halfPenWidth,
                    LineTypeIndexOffset + halfPenWidth
                }))
            {
                args.DrawingContext.DrawLine(
                    _gridPen,
                    new Point(NewIndexOffset, args.Rect.Top - 1),
                    new Point(NewIndexOffset, args.Rect.Bottom + 1));

                args.DrawingContext.DrawLine(
                    _gridPen,
                    new Point(LineTypeIndexOffset, args.Rect.Top - 1),
                    new Point(LineTypeIndexOffset, args.Rect.Bottom + 1));
            }

            if (args.Index == 0)
                DrawFileTypeMark(args.DrawingContext, args.Rect, args.DiffLine);
        }

        private static void DrawIndex(DrawingContext dc, Rect rect, DiffLine diffLine)
        {
            switch(diffLine.LineType)
            {
                case DiffLine.LineTypes.ChunkTag:
                    DrawIndexText(dc, rect, "･･･", OldIndexOffset, TextAlignment.Center);
                    DrawIndexText(dc, rect, "･･･", NewIndexOffset, TextAlignment.Center);
                    break;

                case DiffLine.LineTypes.Normal:
                    DrawIndexText(dc, rect, diffLine.OldIndex, OldIndexOffset, TextAlignment.Right);
                    DrawIndexText(dc, rect, diffLine.NewIndex, NewIndexOffset, TextAlignment.Right);
                    break;

                case DiffLine.LineTypes.Add:
                    DrawIndexText(dc, rect, diffLine.NewIndex, NewIndexOffset, TextAlignment.Right);
                    break;

                case DiffLine.LineTypes.Delete:
                    DrawIndexText(dc, rect, diffLine.OldIndex, OldIndexOffset, TextAlignment.Right);
                    break;
            }
        }

        private static void DrawFileTypeMark(DrawingContext dc, Rect rect, DiffLine diffLine)
        {
            string mark;
            Brush brush;
            if (diffLine.LineType == DiffLine.LineTypes.Add)
            {
                mark = "+";
                brush = Brushes.Green;
            }
            else if (diffLine.LineType == DiffLine.LineTypes.Delete)
            {
                mark = "-";
                brush = Brushes.Red;
            }
            else
                return;

            dc.DrawText(
                new FormattedText(
                    mark,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"),
                    14,
                    brush),
                new Point(rect.Left + 4, rect.Top - 1));
        }

        private static void DrawIndexText(DrawingContext dc, Rect rect, string text, double x, TextAlignment align)
        {
            var ft =
                new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"),
                    12,
                    Brushes.DimGray);

            double offset;
            {
                if (align == TextAlignment.Center)
                    offset = (IndexWidth - ft.Width) * 0.5;

                else if (align == TextAlignment.Right)
                    offset = IndexWidth - ft.Width - 4;

                else
                    throw new NotImplementedException();
            }

            dc.DrawText(ft, new Point(rect.Left + x + offset, rect.Top));
        }

        private static void DrawIndexText(DrawingContext dc, Rect rect, int index, double x, TextAlignment align)
        {
            DrawIndexText(dc, rect, index.ToString(), x, align);
        }
    }
}