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
using Repository = Anne.Model.Git.Repository;

namespace Anne.Features
{
    public class WipFileVm : ViewModelBase, IFileDiffVm
    {
        // IFileDiffVm
        public ReactiveProperty<string> Path { get; }
        public ReactiveProperty<string> Diff { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<DiffLine[]> DiffLines { get; } = new ReactiveProperty<DiffLine[]>();
        public ReactiveProperty<int> LinesAdded { get; }
        public ReactiveProperty<int> LinesDeleted { get; }
        public ReactiveProperty<ChangeKind> Status { get; }
        public ReactiveProperty<bool> IsBinary { get; }

        public ReactiveProperty<bool> IsInStaging { get; }

        private ReadOnlyReactiveProperty<bool> IsInStagingFromModel { get; }

        public ReactiveCommand DiscardChangesCommand { get; }

        public ReactiveProperty<bool> IsSelected { get; } 

        public WipFileVm(Repository repos, WipFile model)
            : base(true)
        {
            Debug.Assert(repos != null);
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
                .Where(_ => IsBinary.Value == false)
                .Subscribe(this.MakeDiff)
                .AddTo(MultipleDisposable);

            #region IsInStaging, IsInStagingFromModel

            IsInStaging = new ReactiveProperty<bool>(model.IsInStaging)
                .AddTo(MultipleDisposable);

            IsInStaging.Subscribe(x =>
            {
                if (x)
                    model.Stage();
                else
                    model.Unstage();
            }).AddTo(MultipleDisposable);

            IsInStagingFromModel = model.ObserveProperty(x => x.IsInStaging)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            IsInStagingFromModel
                .Subscribe(x => IsInStaging.Value = x)
                .AddTo(MultipleDisposable);

            #endregion

            IsSelected = new ReactiveProperty<bool>()
                .AddTo(MultipleDisposable);

            DiscardChangesCommand = new ReactiveCommand()
                .AddTo(MultipleDisposable);

            DiscardChangesCommand
                .Subscribe(_ => repos.DiscardChanges(new[] {Path.Value}))
                .AddTo(MultipleDisposable);
        }
    }
}