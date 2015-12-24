using System;
using System.Windows;
using System.Windows.Media;
using Anne.Foundation;
using ICSharpCode.AvalonEdit.Rendering;

namespace Anne.Features
{
    public static class FileDiffTextEditorHelper
    {
        public static void DrawBackground(
            TextView textView, DrawingContext dc, double x, double width, FileDiffVm.DiffLine[] diffLines,
            bool isLight,
            Action<TextView, DrawingContext, Rect, FileDiffVm.DiffLine, int> perLineDraw = null)
        {
            if (diffLines == null)
                return;

            foreach (var v in textView.VisualLines)
            {
                var linenum = v.FirstDocumentLine.LineNumber - 1;
                if (linenum >= diffLines.Length)
                    continue;

                var diffLine = diffLines[linenum];
                var brush = default(Brush);

                switch (diffLine.LineType)
                {
                    case FileDiffVm.DiffLine.LineTypes.ChunckTag:
                        brush = isLight
                            ? Constants.LightChunckTagBackground
                            : Constants.ChunckTagBackground;
                        break;

                    case FileDiffVm.DiffLine.LineTypes.Add:
                        brush = isLight
                            ? Constants.LightAddBackground
                            : Constants.AddBackground;
                        break;

                    case FileDiffVm.DiffLine.LineTypes.Delete:
                        brush = isLight
                            ? Constants.LightRemoveBackground
                            : Constants.RemoveBackground;
                        break;
                }

                var index = 0;
                foreach (var segRect in BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, v, 0, 1000))
                {
                    var rect = new Rect(x, segRect.Top, width, segRect.Height + 1);

                    // ReSharper disable once PossibleUnintendedReferenceComparison
                    if (brush != default(Brush))
                        dc.DrawRectangle(brush, null, rect);

                    perLineDraw?.Invoke(textView, dc, rect, diffLine, index ++);
                }
            }
        }
    }
}