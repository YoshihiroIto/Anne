using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Branch
{
    public class BranchVm : ViewModelBase
    {
        public ReadOnlyReactiveProperty<string> Name { get; }
        public ReadOnlyReactiveProperty<bool> IsRemote { get; }
        public ReadOnlyReactiveProperty<bool> IsCurrent { get; }

        public BranchVm(Model.Git.Branch model)
        {
            Name = model.Name.ToReadOnlyReactiveProperty().AddTo(MultipleDisposable);
            IsRemote = model.IsRemote.ToReadOnlyReactiveProperty().AddTo(MultipleDisposable);
            IsCurrent = model.IsCurrent.ToReadOnlyReactiveProperty().AddTo(MultipleDisposable);
        }
    }
}