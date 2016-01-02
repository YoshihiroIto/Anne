using System.Diagnostics;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class RepositoryOutlinerVm : ReposOutlinerHasChildrenItemVm
    {
        private RepositoryVm _repos;

        public RepositoryOutlinerVm(RepositoryVm repos)
            : base(string.Empty)
        {
            Debug.Assert(repos != null);

            _repos = repos;

            var localBranch = new ReposOutlinerRoot("Local").AddTo(MultipleDisposable);
            var remoteBranch = new ReposOutlinerRoot("Remote").AddTo(MultipleDisposable);

            Children.AddOnScheduler(localBranch);
            Children.AddOnScheduler(remoteBranch);
        }
    }
}