using System.Diagnostics;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace Anne.Features
{
    public class FileDiffTextEditorBackgroundRenderer : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;

        private readonly FileDiffTextEditor _editor;
        private FileDiffVm.DiffLine[] DiffLines => ((FileDiffVm) _editor?.DataContext)?.DiffLines;

        public FileDiffTextEditorBackgroundRenderer(FileDiffTextEditor editor)
        {
            Debug.Assert(editor != null);

            _editor = editor;
        }

        public void Draw(TextView textView, DrawingContext dc)
        {
            FileDiffTextEditorHelper.DrawBackground(textView, dc, 0, textView.ActualWidth, DiffLines, true);
        }
    }
}