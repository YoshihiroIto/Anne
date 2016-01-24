using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media;
using Anne.Features.Interfaces;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Features
{
    public class WipCommitVm : ViewModelBase, ICommitVm
    {
        // ICommitVm
        public string Summary
        {
            get { return _summary; }
            set { SetProperty(ref _summary, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public ReactiveCommand CommitCommand { get; }
        public ReactiveCommand DiscardChangesCommand { get; }

        public ReadOnlyReactiveProperty<IEnumerable<WipFileVm>> WipFiles => _repos.FileStatus.WipFiles;

        public ObservableCollection<WipFileVm> SelectedWipFiles { get; }

        private object _diffFileViewSource;
        public object DiffFileViewSource
        {
            get { return _diffFileViewSource; }
            set { SetProperty(ref _diffFileViewSource, value); }
        }

        public ReadOnlyReactiveProperty<int> SummaryRemaining { get; }
        public ReadOnlyReactiveProperty<SolidColorBrush> SummaryRemainingBrush { get; }

        public ReactiveProperty<bool?> IsAllSelected { get; }

        public TwoPaneLayoutVm TwoPaneLayout { get; }

        private readonly RepositoryVm _repos;
        private string _summary = string.Empty;
        private string _description = string.Empty;

        public WipCommitVm(RepositoryVm repos, TwoPaneLayoutVm twoPaneLayout)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            TwoPaneLayout = twoPaneLayout ?? new TwoPaneLayoutVm().AddTo(MultipleDisposable);

            SelectedWipFiles = new ObservableCollection<WipFileVm>();
            SelectedWipFiles.CollectionChangedAsObservable()
                .Subscribe(_ =>
                {
                    var count = SelectedWipFiles.Count;

                    if (count == 0)
                        DiffFileViewSource = WipFiles.Value.FirstOrDefault();
                    else if (count == 1)
                        DiffFileViewSource = SelectedWipFiles.FirstOrDefault();
                    else
                        DiffFileViewSource = SelectedWipFiles;
                })
                .AddTo(MultipleDisposable);


            SummaryRemaining =
                this.ObserveProperty(x => x.Summary)
                    .Select(x => 80 - x.Length)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(MultipleDisposable);

            SummaryRemainingBrush = SummaryRemaining
                .Select(x =>
                {
                    if (x < 0)
                        return Brushes.Red;
                    if (x < 20)
                        return Brushes.DarkRed;
                    return Brushes.Gray;
                })
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            CommitCommand =
                this.ObserveProperty(x => x.Summary)
                    .Select(x => string.IsNullOrWhiteSpace(x) == false)
                    .ToReactiveCommand()
                    .AddTo(MultipleDisposable);

            CommitCommand.Subscribe(_ => repos.Commit((Summary + "\n\n" + Description).Trim()))
                .AddTo(MultipleDisposable);

            DiscardChangesCommand = SelectedWipFiles.CollectionChangedAsObservable()
                .Select(x => SelectedWipFiles.Any())
                .ToReactiveCommand()
                .AddTo(MultipleDisposable);

            DiscardChangesCommand.Subscribe(_ =>
                repos.DiscardChanges(SelectedWipFiles.Select(x => x.Path.Value))
                ).AddTo(MultipleDisposable);

            // IsAllSelected 関係
            {
                IsAllSelected = new ReactiveProperty<bool?>().AddTo(MultipleDisposable);
                IsAllSelected.Subscribe(i =>
                {
                    // UI からの操作では on/off のどちらかに設定する
                    if (_isInUpdateIsAllSelected == false)
                    {
                        if (i.HasValue == false)
                        {
                            IsAllSelected.Value = false;
                            return;
                        }
                    }

                    if (i.HasValue == false)
                        return;

                    WipFiles.Value.ForEach(x => x.IsInStaging.Value = i.Value);
                }).AddTo(MultipleDisposable);

                this.ObserveProperty(x => x.WipFiles)
                    .Subscribe(x =>
                    {
                        _isInStageingDisposer?.Dispose();
                        _isInStageingDisposer = new MultipleDisposable();

                        x.Value.ForEach(y =>
                            y.IsInStaging.Subscribe(_ => UpdateIsAllSelected()).AddTo(_isInStageingDisposer));
                    })
                    .AddTo(MultipleDisposable);

                MultipleDisposable.Add(() => _isInStageingDisposer?.Dispose());
            }
        }

        private MultipleDisposable _isInStageingDisposer;

        private bool _isInUpdateIsAllSelected;
        private void UpdateIsAllSelected()
        {
            using (new AnonymousDisposable(() => _isInUpdateIsAllSelected = false))
            {
                _isInUpdateIsAllSelected = true;

                var isChecked = WipFiles.Value.All(y => y.IsInStaging.Value);
                var isUnchecked = WipFiles.Value.All(y => y.IsInStaging.Value == false);

                if (isChecked)
                    IsAllSelected.Value = true;
                else if (isUnchecked)
                    IsAllSelected.Value = false;
                else
                    IsAllSelected.Value = null;
            }
        }

        public void ToggleStaging()
        {
            SelectedWipFiles.ForEach(f => f.IsInStaging.Value = !f.IsInStaging.Value);
        }
    }
}