using System.Windows;
using System.Windows.Controls;

namespace Anne.Foundation.Controls.Selectors
{
    public class CommitLabelMarkSelector : DataTemplateSelector
    {
        public DataTemplate Local { get; set; }
        public DataTemplate Remote { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CommitLabelType == false)
                return null;

            switch((CommitLabelType)item)
            {
                case CommitLabelType.LocalBranch:
                    return Local;
                case CommitLabelType.RemoveBranch:
                    return Remote;
            }

            return null;
        }
    }
}