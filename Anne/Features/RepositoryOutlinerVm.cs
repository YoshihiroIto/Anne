using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Anne.Foundation;

namespace Anne.Features
{
    public class RepositoryOutlinerVm : RepositoryOutlinerItemVm
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly RepositoryOutlinerItemVm _localBranch;
        private readonly RepositoryOutlinerItemVm _remoteBranch;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public RepositoryOutlinerVm(RepositoryVm repos)
            : base(string.Empty, RepositoryOutlinerItemType.Root)
        {
            // 各項目のルートノードを配置する
            _localBranch = new RepositoryOutlinerItemVm("Local", RepositoryOutlinerItemType.LocalBranchRoot);
            _remoteBranch = new RepositoryOutlinerItemVm("Remote", RepositoryOutlinerItemType.RemoteBranchRoot);
            Children.AddOnScheduler(_localBranch);
            Children.AddOnScheduler(_remoteBranch);

            UpdateBranchNodes(_localBranch, repos.LocalBranches, false);
            UpdateBranchNodes(_remoteBranch, repos.RemoteBranches, true);
        }

        private void UpdateBranchNodes(RepositoryOutlinerItemVm target, ReadOnlyObservableCollection<BranchVm> source,
            bool isRemote)
        {
            Debug.Assert(target != null);
            Debug.Assert(source != null);

            var leafType = isRemote ? RepositoryOutlinerItemType.RemoteBranch : RepositoryOutlinerItemType.LocalBranch;

            target.Children.Clear();

            source.ForEach(s =>
            {
                var node = target;

                var elements = s.Name.Value.Split('/');
                var folders = elements.Take(elements.Length - 1);
                var leaf = elements.Last();

                // フォルダーを構築する
                var isFirst = true;
                foreach (var f in folders)
                {
                    var nextNode = node.Children.FirstOrDefault(x => x.Caption.Value == f);
                    if (nextNode == null)
                    {
                        var type = RepositoryOutlinerItemType.Folder;

                        if (isRemote && isFirst)
                            type = RepositoryOutlinerItemType.RemoteBranchRepos;

                        nextNode = new RepositoryOutlinerItemVm(f, type);
                        node.Children.Add(nextNode);
                    }

                    node = nextNode;
                    isFirst = false;
                }

                node.Children.Add(new RepositoryOutlinerItemVm(leaf, leafType));
            });
        }
    }
}