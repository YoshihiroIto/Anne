using Anne.Foundation.Controls;

namespace Anne.Features
{
    public class FileDiffTextEditor : TextEditor
    {
        public FileDiffTextEditor()
        {
            TextArea.TextView.BackgroundRenderers.Add(new FileDiffTextEditorBackgroundRenderer(this));
            TextArea.LeftMargins.Add(new FileDiffTextEditorLeftMargin(this));
        }
    }
}