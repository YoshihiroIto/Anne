using System.Windows;
using System.Windows.Controls;
using Anne.Features.Interfaces;

namespace Anne.Diff.Selectors
{
    public class DiffTextEditorSelector : DataTemplateSelector
    {
        public DataTemplate Ascii { get; set; }
        public DataTemplate Binary { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fileDiff = item as IFileDiffVm;
            if (fileDiff == null)
                return null;

            return fileDiff.IsBinary ? Binary : Ascii;
        } 
    }
}