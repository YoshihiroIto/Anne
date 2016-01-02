using System.Windows;
using System.Windows.Controls;

namespace Anne.Foundation.Controls.Selectors
{
    public class RepositoryOutlinerItemMarkSelector : DataTemplateSelector
    {
        public DataTemplate Root { get; set; }
        public DataTemplate LocalBranchRoot { get; set; }
        public DataTemplate RemoteBranchRoot { get; set; }
        public DataTemplate RemoteBranchRepos { get; set; }
        public DataTemplate LocalBranch { get; set; }
        public DataTemplate RemoteBranch { get; set; }
        public DataTemplate Folder { get; set; }
        public DataTemplate Current { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is RepositoryOutlinerItemType == false)
                return null;

            switch ((RepositoryOutlinerItemType) item)
            {
                case RepositoryOutlinerItemType.Root:
                    return Root;
                case RepositoryOutlinerItemType.LocalBranchRoot:
                    return LocalBranchRoot;
                case RepositoryOutlinerItemType.RemoteBranchRoot:
                    return RemoteBranchRoot;
                case RepositoryOutlinerItemType.RemoteBranchRepos:
                    return RemoteBranchRepos;
                case RepositoryOutlinerItemType.LocalBranch:
                    return LocalBranch;
                case RepositoryOutlinerItemType.RemoteBranch:
                    return RemoteBranch;
                case RepositoryOutlinerItemType.Folder:
                    return Folder;
                case RepositoryOutlinerItemType.Current:
                    return Current;
            }

            return null;
        }
    }
}