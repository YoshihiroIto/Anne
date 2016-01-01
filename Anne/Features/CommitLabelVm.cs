using System.Diagnostics;
using System.Windows.Media;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class CommitLabelVm : ViewModelBase
    {
        public ReadOnlyReactiveProperty<string> Name { get; }
        public ReadOnlyReactiveProperty<CommitLabelType> Type { get; }
        public ReactiveProperty<Brush> Background { get; }

        public CommitLabelVm(Model.Git.CommitLabel model)
        {
            Debug.Assert(model != null);

            Name = model
                .ObserveProperty(x => x.Name)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            Type = model
                .ObserveProperty(x => x.Type)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            Background = new ReactiveProperty<Brush>(Brushes.MistyRose)
                .AddTo(MultipleDisposable);
        }
    }
}