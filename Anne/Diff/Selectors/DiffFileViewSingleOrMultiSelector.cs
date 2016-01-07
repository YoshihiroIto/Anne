using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Anne.Diff.Selectors
{
    public class DiffFileViewSingleOrMultiSelector : DataTemplateSelector
    {
        public DataTemplate Single { get; set; }
        public DataTemplate Multi { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item is IList ? Multi : Single;
        } 
    }
}