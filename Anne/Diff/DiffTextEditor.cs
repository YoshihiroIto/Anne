using Anne.Foundation.Controls;

namespace Anne.Diff
{
    public class DiffTextEditor : TextEditor
    {
        public DiffTextEditor()
        {
            TextArea.TextView.BackgroundRenderers.Add(new DiffTextEditorBackgroundRenderer(this));
            TextArea.LeftMargins.Add(new DiffTextEditorLeftMargin(this));

            IsTabStop = false;
            Focusable = false;
            TextArea.IsTabStop = false;
            TextArea.Focusable = false;
        }
    }
}