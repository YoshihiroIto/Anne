using System;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Model
{
    public class Repository : ModelBase
    {
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();

        private LibGit2Sharp.Repository _repository;

        public Repository()
        {
            Path.AddTo(MultipleDisposable);

            Path.Subscribe(p =>
            {
                MultipleDisposable.RemoveAndDispoes(_repository);
                _repository = new LibGit2Sharp.Repository(p);
            });
        }
    }
}
