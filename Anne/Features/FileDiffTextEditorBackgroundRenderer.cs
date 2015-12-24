using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Anne.Foundation;
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

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            FileDiffTextEditorHelper.DrawBackground(textView, drawingContext, textView.ActualWidth, DiffLines);
        }
    }
}