using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class ChangingFileVm : ViewModelBase
    {
        public string Path => _model.Path;
        public ReactiveProperty<bool>  IsInStaging { get; }

        private ReadOnlyReactiveProperty<bool> IsInStagingFromModel { get; }

        private readonly ChangingFile _model;

        public ChangingFileVm(ChangingFile model)
            : base(true)
        {
            Debug.Assert(model != null);
            _model = model;

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
        }
    }
}