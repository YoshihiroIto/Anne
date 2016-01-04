using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Anne.Foundation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class RepositoryOutlinerVm : RepositoryOutlinerItemVm
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly RepositoryOutlinerItemVm _localBranch;
        private readonly RepositoryOutlinerItemVm _remoteBranch;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public ReactiveProperty<RepositoryOutlinerItemVm> SelectedItem { get; }

        public ReactiveCommand SwitchBranchCommand { get; }

        private readonly RepositoryVm _repos;

        public RepositoryOutlinerVm(RepositoryVm repos)
            : base(string.Empty, RepositoryOutlinerItemType.Root, null, repos)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            SelectedItem = new ReactiveProperty<RepositoryOutlinerItemVm>()
                .AddTo(MultipleDisposable);

            // 各項目のルートノードを配置する
            _localBranch =
                new RepositoryOutlinerItemVm("Local", RepositoryOutlinerItemType.LocalBranchRoot, null, repos)
                    .AddTo(MultipleDisposable);

            _remoteBranch =
                new RepositoryOutlinerItemVm("Remote", RepositoryOutlinerItemType.RemoteBranchRoot, null, repos)
                    .AddTo(MultipleDisposable);

            Children.AddOnScheduler(_localBranch);
            Children.AddOnScheduler(_remoteBranch);

            UpdateBranchNodes(_localBranch, repos.LocalBranches, false);
            UpdateBranchNodes(_remoteBranch, repos.RemoteBranches, true);

            repos.LocalBranches.CollectionChangedAsObservable()
                .Subscribe(_ => UpdateBranchNodes(_localBranch, repos.LocalBranches, false))
                .AddTo(MultipleDisposable);

            repos.RemoteBranches.CollectionChangedAsObservable()
                .Subscribe(_ => UpdateBranchNodes(_remoteBranch, repos.RemoteBranches, true))
                .AddTo(MultipleDisposable);

            SwitchBranchCommand = new ReactiveCommand().AddTo(MultipleDisposable);
            SwitchBranchCommand.Subscribe(_ => SwitchBranch()).AddTo(MultipleDisposable);
        }

        private void UpdateBranchNodes(RepositoryOutlinerItemVm target, ReadOnlyObservableCollection<BranchVm> source,
            bool isRemote)
        {
            Debug.Assert(target != null);
            Debug.Assert(source != null);

            var leafType = isRemote
                ? RepositoryOutlinerItemType.RemoteBranch
                : RepositoryOutlinerItemType.LocalBranch;

            target.Children.ForEach(x => x.Dispose());
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

                        nextNode = new RepositoryOutlinerItemVm(f, type, null, _repos)
                        {
                            IsExpanded = {Value = isFirst}
                        };
                        node.Children.Add(nextNode);
                    }

                    node = nextNode;
                    isFirst = false;
                }

                node.Children.Add(new RepositoryOutlinerItemVm(leaf, leafType, s, _repos));
            });
        }

        private void SwitchBranch()
        {
            var selectedBranch = SelectedItem.Value?.Branch;

            if (selectedBranch?.IsRemote.Value == true)
            {
                _repos.CheckoutBranch(selectedBranch.Name.Value);
            }
            else if (selectedBranch?.IsRemote.Value == false)
            {
                if (selectedBranch.IsCurrent.Value == false)
                    _repos.SwitchBranch(selectedBranch.Name.Value);
            }
        }
    }
}