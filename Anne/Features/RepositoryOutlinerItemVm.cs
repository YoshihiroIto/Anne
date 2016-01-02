using System;
using System.Linq;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Features
{
#if false
    public enum ReposOutlinerItemType
    {
        //
        LocalBranchRoot,
        RemoteBranchRoot,
        //
        RemoteBranchRepos,
        //
        LocalBranch,
        RemoteBranch,
        //
        Folder
    }
#endif

    public class ReposOutlinerItemVm : ViewModelBase
    {
        public ReactiveProperty<string> Caption { get; }
            = new ReactiveProperty<string>();

        public ReposOutlinerItemVm(string caption)
        {
            Caption.AddTo(MultipleDisposable);

            Caption.Value = caption;
        }
    }

    public class ReposOutlinerHasChildrenItemVm : ReposOutlinerItemVm
    {
        public ReactiveCollection<ReposOutlinerItemVm> Children { get; }
            = new ReactiveCollection<ReposOutlinerItemVm>();

        public ReposOutlinerHasChildrenItemVm(string caption)
            : base(caption)
        {
            Children.AddTo(MultipleDisposable);
            new AnonymousDisposable(() => Children.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);
        }
    }

    public class ReposOutlinerRoot : ReposOutlinerHasChildrenItemVm
    {
        public ReposOutlinerRoot(string caption)
            : base(caption)
        {
        }
    }
}