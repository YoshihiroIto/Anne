using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Model.Git
{
    public class Branch : ModelBase
    {
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();

        public Branch(LibGit2Sharp.Branch src)
        {
            Name.AddTo(MultipleDisposable);

            Name.Value = src.Name;
        }
    } 
}