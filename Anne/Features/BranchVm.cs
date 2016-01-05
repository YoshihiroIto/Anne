using System.Diagnostics;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class BranchVm : ViewModelBase
    {
        public ReadOnlyReactiveProperty<string> Name { get; }
        public ReadOnlyReactiveProperty<string> LocalName { get; }
        public ReadOnlyReactiveProperty<string> RemoteName { get; }
        public ReadOnlyReactiveProperty<string> CanonicalName { get; }
        public ReadOnlyReactiveProperty<bool> IsRemote { get; }
        public ReadOnlyReactiveProperty<bool> IsCurrent { get; }

        public BranchVm(RepositoryVm repos, Model.Git.Branch model)
        {
            Debug.Assert(repos != null);
            Debug.Assert(model != null);

            Name = model
                .ObserveProperty(x => x.Name)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            LocalName = model
                .ObserveProperty(x => x.LocalName)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            RemoteName = model
                .ObserveProperty(x => x.RemoteName)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            CanonicalName = model
                .ObserveProperty(x => x.CanonicalName)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            IsRemote = model
                .ObserveProperty(x => x.IsRemote)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            IsCurrent = model
                .ObserveProperty(x => x.IsCurrent)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);
        }
    }
}