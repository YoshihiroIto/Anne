using System.Diagnostics;
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
        public ReactiveProperty<DiffLine[]> DiffLines { get; } = new ReactiveProperty<DiffLine[]>();
        public ReactiveProperty<int> LinesAdded { get; }
        public ReactiveProperty<int> LinesDeleted { get; }
        public ReactiveProperty<ChangeKind> Status { get; }
        public ReactiveProperty<bool> IsBinary { get; }

        private string _diff;

        public string Diff
        {
            get
            {
                if (_diff == null)
                {
                    if (IsBinary.Value)
                        _diff = string.Empty;
                    else
                    {
                        this.MakeDiff(_model.Patch);
                        _model = null;
                    }
                }

                return _diff;
            }

            set { SetProperty(ref _diff, value); }
        }

        private ChangeFile _model;

        public ChangeFileVm(ChangeFile model)
        {
            Debug.Assert(model != null);

            _model = model;

            //
            Path = model.ObserveProperty(x => x.Path).ToReactiveProperty().AddTo(MultipleDisposable);
            Status = model.ObserveProperty(x => x.Status).ToReactiveProperty().AddTo(MultipleDisposable);
            //
            DiffLines.AddTo(MultipleDisposable);
            //
            LinesAdded = model.ObserveProperty(x => x.LinesAdded, false).ToReactiveProperty().AddTo(MultipleDisposable);
            LinesDeleted = model.ObserveProperty(x => x.LinesDeleted, false).ToReactiveProperty().AddTo(MultipleDisposable);
            IsBinary = model.ObserveProperty(x => x.IsBinary, false).ToReactiveProperty().AddTo(MultipleDisposable);
        }
    }
}