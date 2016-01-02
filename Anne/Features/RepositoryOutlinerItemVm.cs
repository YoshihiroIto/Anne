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

        public RepositoryOutlinerItemVm(string caption, RepositoryOutlinerItemType type)
        {
            Type = type;

            Caption.AddTo(MultipleDisposable);
            Caption.Value = caption;

            IsExpanded.AddTo(MultipleDisposable);
            IsExpanded.Value =
                type == RepositoryOutlinerItemType.LocalBranchRoot ||
                type == RepositoryOutlinerItemType.RemoteBranchRoot;

            Children.AddTo(MultipleDisposable);
            new AnonymousDisposable(() => Children.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);
        }
    }
}