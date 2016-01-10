using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Anne.Diff;
using Anne.Features.Interfaces;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class ChangeFileVm : ViewModelBase, IFileDiffVm
    {
        // IFileDiffVm
        public ReactiveProperty<string> Path { get; }
        public ReactiveProperty<string> Diff { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<DiffLine[]> DiffLines { get; } = new ReactiveProperty<DiffLine[]>();
        public ReactiveProperty<int> LinesAdded { get; }
        public ReactiveProperty<int> LinesDeleted { get; }
        public ReactiveProperty<ChangeKind> Status { get; }
        public ReactiveProperty<bool> IsBinary { get; }

        public ChangeFileVm(ChangeFile model)
        {
            Debug.Assert(model != null);

            Path = model.ObserveProperty(x => x.Path).ToReactiveProperty().AddTo(MultipleDisposable);
            Diff.AddTo(MultipleDisposable);
            DiffLines.AddTo(MultipleDisposable);

            LinesAdded = model.ObserveProperty(x => x.LinesAdded).ToReactiveProperty().AddTo(MultipleDisposable);
            LinesDeleted = model.ObserveProperty(x => x.LinesDeleted).ToReactiveProperty().AddTo(MultipleDisposable);
            Status = model.ObserveProperty(x => x.Status).ToReactiveProperty().AddTo(MultipleDisposable);
            IsBinary = model.ObserveProperty(x => x.IsBinary).ToReactiveProperty().AddTo(MultipleDisposable);

            IsBinary
                .Where(i => i == false)
                .Subscribe(_ => this.MakeDiff(model.Patch))
                .AddTo(MultipleDisposable);

            model.ObserveProperty(x => x.Patch)
                .Subscribe(this.MakeDiff)
                .AddTo(MultipleDisposable);
        }
    }
}