using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Anne.Foundation;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace Anne.Features
{
    public class FileDiffTextEditorLeftMargin : AbstractMargin
    {
        private readonly FileDiffTextEditor _editor;
        private FileDiffVm.DiffLine[] DiffLines => ((FileDiffVm) _editor?.DataContext)?.DiffLines;

        private const double IndexWidth = 40;
        private const double LineTypeWidth = 16;

        private const double MarginWidth = IndexWidth + IndexWidth + LineTypeWidth;
        private const double OldIndexOffset = 0;
        private const double NewIndexOffset = IndexWidth;
        private const double LineTypeIndexOffset = IndexWidth + IndexWidth;

        public FileDiffTextEditorLeftMargin(FileDiffTextEditor editor)
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
            FileDiffTextEditorHelper.DrawBackground(TextView, dc, x0, w0, DiffLines, false, DrawForegroundLeft);

            const double x1 = IndexWidth*2;
            const double w1 = LineTypeWidth;
            FileDiffTextEditorHelper.DrawBackground(TextView, dc, x1, w1, DiffLines, true, DrawForeground);
        }

        private static void DrawForegroundLeft(FileDiffTextEditorHelper.PerLineDrawArgs args)
        {
            if (args.Index == 0)
                DrawIndex(args.DrawingContext, args.Rect, args.DiffLine);
        }

        private static void DrawForeground(FileDiffTextEditorHelper.PerLineDrawArgs args)
        {
            args.DrawingContext.DrawLine(
                Constants.FramePen,
                new Point(OldIndexOffset, args.Rect.Top),
                new Point(OldIndexOffset, args.Rect.Bottom));

            args.DrawingContext.DrawLine(
                Constants.FramePen,
                new Point(NewIndexOffset, args.Rect.Top),
                new Point(NewIndexOffset, args.Rect.Bottom));

            args.DrawingContext.DrawLine(
                Constants.FramePen,
                new Point(LineTypeIndexOffset, args.Rect.Top),
                new Point(LineTypeIndexOffset, args.Rect.Bottom));

            if (args.Index == 0)
                DrawFileTypeMark(args.DrawingContext, args.Rect, args.DiffLine);
        }

        private static void DrawIndex(DrawingContext dc, Rect rect, FileDiffVm.DiffLine diffLine)
        {
            switch(diffLine.LineType)
            {
                case FileDiffVm.DiffLine.LineTypes.ChunckTag:
                    DrawIndexText(dc, rect, "･･･", OldIndexOffset, TextAlignment.Center);
                    DrawIndexText(dc, rect, "･･･", NewIndexOffset, TextAlignment.Center);
                    break;

                case FileDiffVm.DiffLine.LineTypes.Normal:
                    DrawIndexText(dc, rect, diffLine.OldIndex, OldIndexOffset, TextAlignment.Right);
                    DrawIndexText(dc, rect, diffLine.NewIndex, NewIndexOffset, TextAlignment.Right);
                    break;

                case FileDiffVm.DiffLine.LineTypes.Add:
                    DrawIndexText(dc, rect, diffLine.NewIndex, NewIndexOffset, TextAlignment.Right);
                    break;

                case FileDiffVm.DiffLine.LineTypes.Delete:
                    DrawIndexText(dc, rect, diffLine.OldIndex, OldIndexOffset, TextAlignment.Right);
                    break;
            }
        }

        private static void DrawFileTypeMark(DrawingContext dc, Rect rect, FileDiffVm.DiffLine diffLine)
        {
            string mark;
            Brush brush;
            if (diffLine.LineType == FileDiffVm.DiffLine.LineTypes.Add)
            {
                mark = "+";
                brush = Brushes.Green;
            }
            else if (diffLine.LineType == FileDiffVm.DiffLine.LineTypes.Delete)
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
                new Point(rect.Left + 4, rect.Top));
        }

        private static void DrawIndexText(DrawingContext dc, Rect rect, string text, double x, TextAlignment align)
        {
            var ft =
                new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"),
                    14,
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