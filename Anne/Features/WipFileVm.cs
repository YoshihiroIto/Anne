using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using Anne.Diff;
using Anne.Features.Interfaces;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Repository = Anne.Model.Git.Repository;

namespace Anne.Features
{
    public class WipFileVm : ViewModelBase, IFileDiffVm
    {
        // IFileDiffVm
        public string Path => _model.Path;
        public string Diff { get; set; }
        public DiffLine[] DiffLines { get; set; }

        public ReactiveProperty<bool> IsInStaging { get; }

        private readonly WipFile _model;
        private ReadOnlyReactiveProperty<bool> IsInStagingFromModel { get; }

        public WipFileVm(Repository repos, WipFile model)
            : base(true)
        {
            Debug.Assert(repos != null);
            Debug.Assert(model != null);
            _model = model;

            #region IsInStaging, IsInStagingFromModel

            IsInStaging = new ReactiveProperty<bool>(Scheduler.Immediate, model.IsInStaging)
                .AddTo(MultipleDisposable);

            IsInStaging.Subscribe(x =>
            {
                if (x)
                    model.Stage();
                else
                    model.Unstage();
            })
                .AddTo(MultipleDisposable);

            IsInStagingFromModel = model.ObserveProperty(x => x.IsInStaging)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            IsInStagingFromModel
                .Subscribe(x => IsInStaging.Value = x)
                .AddTo(MultipleDisposable);

            #endregion

            this.MakeDiff(_model.Patch);
        }
    }
}