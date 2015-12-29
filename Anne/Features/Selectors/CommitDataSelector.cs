using System.Windows;
using System.Windows.Controls;

namespace Anne.Features.Selectors
{
    public class CommitDataSelector : DataTemplateSelector
    {
        public DataTemplate Done { get; set; }
        public DataTemplate Wip { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DoneCommitVm)
                return Done;

            if (item is WipCommitVm)
                return Wip;

            return null;
        }
    }
}