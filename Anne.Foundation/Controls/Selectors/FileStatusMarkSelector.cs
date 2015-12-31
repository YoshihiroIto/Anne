using System.Windows;
using System.Windows.Controls;
using LibGit2Sharp;

namespace Anne.Foundation.Controls.Selectors
{
    public class FileStatusMarkSelector : DataTemplateSelector
    {
        public DataTemplate Added { get; set; }
        public DataTemplate Deleted { get; set; }
        public DataTemplate Modified { get; set; }
        public DataTemplate Renamed { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChangeKind == false)
                return null;

            switch((ChangeKind)item)
            {
                case ChangeKind.Added:
                    return Added;
                case ChangeKind.Deleted:
                    return Deleted;
                case ChangeKind.Modified:
                    return Modified;
                case ChangeKind.Renamed:
                    return Renamed;
            }

            return null;
        }
    }
}