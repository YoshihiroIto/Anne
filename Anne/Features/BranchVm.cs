﻿using System.Diagnostics;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class BranchVm : ViewModelBase
    {
        public ReadOnlyReactiveProperty<string> Name { get; }
        public ReadOnlyReactiveProperty<bool> IsRemote { get; }
        public ReadOnlyReactiveProperty<bool> IsCurrent { get; }

        public BranchVm(Model.Git.Branch model)
        {
            Debug.Assert(model != null);

            Name = model
                .ObserveProperty(x => x.Name)
                .ToReadOnlyReactiveProperty(eventScheduler:UIDispatcherScheduler.Default)
                .AddTo(MultipleDisposable);

            IsRemote = model
                .ObserveProperty(x => x.IsRemote)
                .ToReadOnlyReactiveProperty(eventScheduler:UIDispatcherScheduler.Default)
                .AddTo(MultipleDisposable);

            IsCurrent = model
                .ObserveProperty(x => x.IsCurrent)
                .ToReadOnlyReactiveProperty(eventScheduler:UIDispatcherScheduler.Default)
                .AddTo(MultipleDisposable);
        }
    }
}