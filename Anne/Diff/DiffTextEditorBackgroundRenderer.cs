using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using Anne.Features.Interfaces;
using Anne.Foundation;
using ICSharpCode.AvalonEdit.Rendering;

namespace Anne.Diff
{
    public class DiffTextEditorBackgroundRenderer : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;

        private readonly DiffTextEditor _editor;
        private DiffLine[] DiffLines => ((IFileDiffVm) _editor?.DataContext)?.DiffLines.Value;

        public DiffTextEditorBackgroundRenderer(DiffTextEditor editor)
        {
            Debug.Assert(editor != null);

            _editor = editor;
        }

        public void Draw(TextView textView, DrawingContext dc)
        {
            DiffTextEditorHelper.DrawBackground(
                textView, dc, 0, textView.ActualWidth, DiffLines, true, DrawContentDiff);
        }

        private static void DrawContentDiff(DiffTextEditorHelper.PerLineDrawArgs args)
        {
            if (args.DiffLine.ContentDiffs == null)
                return;

            var brush = args.DiffLine.LineType == DiffLine.LineTypes.Add
                ? Constants.HighlightAddBackground
                : Constants.HighlightRemoveBackground;

            args.DiffLine.ContentDiffs.SelectMany(d =>
                BackgroundGeometryBuilder.GetRectsFromVisualSegment(
                    args.TextView,
                    args.VisualLine,
                    d.StartIndex,
                    d.EndIndex)
                )
                .ForEach(r => args.DrawingContext.DrawRectangle(brush, null, r));
        }
    }
}