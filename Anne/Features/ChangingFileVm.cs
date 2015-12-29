using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using Anne.Features.Interfaces;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Repository = Anne.Model.Git.Repository;

namespace Anne.Features
{
    public class ChangingFileVm : ViewModelBase, IFileDiffVm
    {
        public string Path => _model.Path;

        public string Diff { get; set; }
        public DiffLine[] DiffLines { get; set; }

        public ReactiveProperty<bool> IsInStaging { get; }

        private ReadOnlyReactiveProperty<bool> IsInStagingFromModel { get; }

        private readonly ChangingFile _model;
        private readonly Repository _repos;

        public ChangingFileVm(Repository repos, ChangingFile model)
            : base(true)
        {
            Debug.Assert(repos != null);
            Debug.Assert(model != null);
            _repos = repos;
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

            MakeDiff();
        }

        private void MakeDiff()
        {
            var c = _repos.Internal.Diff.Compare<Patch>(
                _repos.Internal.Head.Tip.Tree,
                DiffTargets.Index | DiffTargets.WorkingDirectory,
                new[] {Path}
                ).FirstOrDefault();

            if (c != null)
                this.MakeDiff(c.Patch);
        }
    }
}