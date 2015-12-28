using System.Windows;
using System.Windows.Controls;

namespace Anne.Features.Selectors
{
    public class CommitDataSelector : DataTemplateSelector
    {
        public DataTemplate Done { get; set; }
        public DataTemplate WorkInProgress { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DoneCommitVm)
                return Done;

            if (item is WorkInProgressCommitVm)
                return WorkInProgress;

            return null;
        }
    }
}