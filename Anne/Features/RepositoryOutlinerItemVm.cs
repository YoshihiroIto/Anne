using System;
using System.Diagnostics;
using System.Linq;
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

        public ReactiveCommand RemoveSelectedBranchesCommand { get; }

        private readonly RepositoryOutlinerVm _parent;

        public RepositoryOutlinerItemVm(string caption, RepositoryOutlinerItemType type, BranchVm branch, RepositoryVm repos, RepositoryOutlinerVm parent)
        {
            Debug.Assert(repos != null);

            _parent = parent;

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

            MultipleDisposable.Add(() => Children.ForEach(x => x.Dispose()));

            RemoveSelectedBranchesCommand = new ReactiveCommand().AddTo(MultipleDisposable);
            RemoveSelectedBranchesCommand.Subscribe(_ => _parent.RemoveSelectedBranches()).AddTo(MultipleDisposable);
        }
    }
}