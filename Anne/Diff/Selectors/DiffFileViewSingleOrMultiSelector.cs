using System.Windows;
using System.Windows.Controls;
using Anne.Features.Interfaces;

namespace Anne.Diff.Selectors
{
    public class DiffFileViewSingleOrMultiSelector : DataTemplateSelector
    {
        public DataTemplate Single { get; set; }
        public DataTemplate Multi { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return Multi;
        } 
    }
}