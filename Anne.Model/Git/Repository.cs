using System;
using System.Reactive.Linq;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Model.Git
{
    public class Repository : ModelBase
    {
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();
        public ReadOnlyReactiveCollection<Branch> Branches { get; private set; }

        public Repository()
        {
            Path
                .AddTo(MultipleDisposable);

            var repositry = Path
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => new LibGit2Sharp.Repository(path))
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            repositry
                .Where(repository => repository != null)
                .Subscribe(repository =>
                {
                    MultipleDisposable.RemoveAndDispoes(Branches);

                    Branches =
                        repository.Branches
                            .ToObservableCollection()
                            .ToReadOnlyReactiveCollection(x => new Branch(x))
                            .AddTo(MultipleDisposable);
                })
                .AddTo(MultipleDisposable);
        }
    }
}