using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Anne.Diff;
using Anne.Features.Interfaces;
using Anne.Foundation;
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
        public ReactiveProperty<DiffLine[]> DiffLines { get; } = new ReactiveProperty<DiffLine[]>();
        public ReactiveProperty<int> LinesAdded { get; }
        public ReactiveProperty<int> LinesDeleted { get; }
        public ReactiveProperty<ChangeKind> Status { get; }
        public ReactiveProperty<bool> IsBinary { get; }

        private SavingMemoryString _diff;

        public SavingMemoryString Diff
        {
            get
            {
                if (_diff != null)
                    return _diff;

                lock (_diffMakingSync)
                {
                    if (IsDiffMaking)
                        return null;

                    IsDiffMaking = true;
                }

                Task.Run(() =>
                {
                    if (_model.IsBinary)
                        _diff = new SavingMemoryString();
                    else
                        this.MakeDiff(_model.Patch);

                    _model = null;

                    IsDiffMaking = false;
                    RaisePropertyChanged();
                    _disposeResetEvent?.Set();
                });

                return _diff;
            }

            set { SetProperty(ref _diff, value); }
        }


        private readonly object _diffMakingSync = new object();
        private bool _isDiffMaking;

        public bool IsDiffMaking
        {
            get { return _isDiffMaking; }
            set { SetProperty(ref _isDiffMaking, value); }
        }

        private ChangeFile _model;
        private ManualResetEventSlim _disposeResetEvent;

        public ChangeFileVm(ChangeFile model)
        {
            Debug.Assert(model != null);

            _model = model;

            MultipleDisposable.AddFirst(() =>
            {
                lock (_diffMakingSync)
                {
                    if (IsDiffMaking)
                        _disposeResetEvent = new ManualResetEventSlim();
                }
            });

            //
            Path = model.ObserveProperty(x => x.Path).ToReactiveProperty().AddTo(MultipleDisposable);
            Status = model.ObserveProperty(x => x.Status).ToReactiveProperty().AddTo(MultipleDisposable);
            //
            DiffLines.AddTo(MultipleDisposable);
            //
            LinesAdded = model.ObserveProperty(x => x.LinesAdded, false).ToReactiveProperty().AddTo(MultipleDisposable);
            LinesDeleted =
                model.ObserveProperty(x => x.LinesDeleted, false).ToReactiveProperty().AddTo(MultipleDisposable);
            IsBinary = model.ObserveProperty(x => x.IsBinary, false).ToReactiveProperty().AddTo(MultipleDisposable);
        }
    }
}