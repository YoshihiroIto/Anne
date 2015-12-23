using System.Diagnostics;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class FilePatchVm : ViewModelBase
    {
        public string Path => _model.Path;
        public string Diff => _model.Diff;

        public ReactiveProperty<bool> IsExpanded { get; } = new ReactiveProperty<bool>(false);

        private readonly FilePatch _model;

        public FilePatchVm(FilePatch model)
        {
            Debug.Assert(model != null);

            _model = model;

            IsExpanded.AddTo(MultipleDisposable);
        }
    }
}