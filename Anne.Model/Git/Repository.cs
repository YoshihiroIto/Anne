using System.Linq;
using System.Reactive.Linq;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Model.Git
{
    public class Repository : ModelBase
    {
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();

        public ReadOnlyReactiveCollection<Branch> LocalBranches { get; }
        public ReadOnlyReactiveCollection<Branch> RemoteBranches { get; }

        public Repository()
        {
            Path
                .AddTo(MultipleDisposable);

            var repositry = Path
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => new LibGit2Sharp.Repository(path))
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            var branches =
                repositry
                    .Where(repository => repository != null)
                    .SelectMany(x => x.Branches);

            LocalBranches =
                branches
                    .Where(b => !b.IsRemote)
                    .Select(x => new Branch(x, repositry.Value))
                    .ToReadOnlyReactiveCollection()
                    .AddTo(MultipleDisposable);

            RemoteBranches =
                branches
                    .Where(b => b.IsRemote)
                    .Select(x => new Branch(x, repositry.Value))
                    .ToReadOnlyReactiveCollection()
                    .AddTo(MultipleDisposable);
        }

        public void CheckoutTest()
        {
            var srcBranch = RemoteBranches.FirstOrDefault(b => b.Name.Value == "origin/refactoring");

            srcBranch?.Checkout();
        }
    }
}