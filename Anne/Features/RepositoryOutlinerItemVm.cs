using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Features
{
    public class RepositoryOutlinerItemVm : ViewModelBase
    {
        public ReactiveProperty<string> Caption { get; }
            = new ReactiveProperty<string>();

        public ReactiveProperty<bool> IsExpanded { get; }
            = new ReactiveProperty<bool>();

        public ReactiveCollection<RepositoryOutlinerItemVm> Children { get; }
            = new ReactiveCollection<RepositoryOutlinerItemVm>();

        public RepositoryOutlinerItemType Type { get; }
        public BranchVm Branch { get; }

        public ReactiveProperty<bool> IsCurrent { get; }

        public ReadOnlyReactiveProperty<string> HistoryDivergence { get; }

        public ReactiveCommand RemoveBranchCommand { get; }

        public RepositoryOutlinerItemVm(string caption, RepositoryOutlinerItemType type, BranchVm branch, RepositoryVm repos)
        {
            Debug.Assert(repos != null);

            Type = type;
            Branch = branch;

            if (Branch != null)
            {
                IsCurrent = branch.IsCurrent
                    .ToReactiveProperty()
                    .AddTo(MultipleDisposable);
            }
            else
            {
                IsCurrent = new ReactiveProperty<bool>(false)
                    .AddTo(MultipleDisposable);
            }

            Caption.AddTo(MultipleDisposable);
            Caption.Value = caption;

            IsExpanded.AddTo(MultipleDisposable);
            IsExpanded.Value =
                type == RepositoryOutlinerItemType.LocalBranchRoot ||
                type == RepositoryOutlinerItemType.RemoteBranchRoot;

            Children.AddTo(MultipleDisposable);
            new AnonymousDisposable(() => Children.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);

            HistoryDivergence = repos.HistoryDivergence
                .Select(h => IsCurrent.Value ? h : string.Empty)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            RemoveBranchCommand = new ReactiveCommand().AddTo(MultipleDisposable);
            RemoveBranchCommand.Subscribe(_ => RemoveBranch()).AddTo(MultipleDisposable);
        }

        private void RemoveBranch()
        {
            if (Branch.IsRemote.Value == false)
                Branch.Remove();
        }
    }
}