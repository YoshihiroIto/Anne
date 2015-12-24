using System.Windows;
using System.Windows.Media;
using Anne.Foundation;
using ICSharpCode.AvalonEdit.Rendering;

namespace Anne.Features
{
    public static class FileDiffTextEditorHelper
    {
        public static void DrawBackground(TextView textView, DrawingContext drawingContext, double width, FileDiffVm.DiffLine[] diffLines)
        {
            if (diffLines == null)
                return;

            foreach (var v in textView.VisualLines)
            {
                var linenum = v.FirstDocumentLine.LineNumber - 1;
                if (linenum >= diffLines.Length)
                    continue;

                var diffLine = diffLines[linenum];
                if (diffLine.LineType == FileDiffVm.DiffLine.LineTypes.Normal)
                    continue;

                var brush = default(Brush);

                switch (diffLine.LineType)
                {
                    case FileDiffVm.DiffLine.LineTypes.ChunckTag:
                        brush = Constants.ChunckTagBackground;
                        break;

                    case FileDiffVm.DiffLine.LineTypes.Add:
                        brush = Constants.AddeBackground;
                        break;

                    case FileDiffVm.DiffLine.LineTypes.Delete:
                        brush = Constants.RemoveBackground;
                        break;
                }

                foreach (var rect in BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, v, 0, 1000))
                {
                    drawingContext.DrawRectangle(
                        brush,
                        null,
                        new Rect(0, rect.Top, width, rect.Height + 1));
                }
            }
        }
    }
}