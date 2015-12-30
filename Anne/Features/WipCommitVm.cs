using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Media;
using Anne.Features.Interfaces;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class WipCommitVm : ViewModelBase, ICommitVm
    {
        // ICommitVm
        public string Summry
        {
            get { return _summry; }
            set { SetProperty(ref _summry, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public ReadOnlyReactiveProperty<IEnumerable<WipFileVm>> WipFiles => _repos.FileStatus.WipFiles;
        public ReactiveCommand CommitCommand { get; }

        public ReactiveProperty<WipFileVm> SelectedWipFile { get; } = new ReactiveProperty<WipFileVm>();
        public ReadOnlyReactiveProperty<int> SummryRemaining { get; }
        public ReadOnlyReactiveProperty<SolidColorBrush> SummryRemainingBrush { get; }

        private readonly RepositoryVm _repos;
        private string _summry = string.Empty;
        private string _description = string.Empty;

        public WipCommitVm(RepositoryVm repos)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            SelectedWipFile
                .AddTo(MultipleDisposable);

            SummryRemaining =
                this.ObserveProperty(x => x.Summry)
                    .Select(x => 80 - x.Length)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(MultipleDisposable);

            SummryRemainingBrush = SummryRemaining
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
                this.ObserveProperty(x => x.Summry)
                    .Select(x => string.IsNullOrWhiteSpace(x) == false)
                    .ToReactiveCommand()
                    .AddTo(MultipleDisposable);

            CommitCommand.Subscribe(_ => repos.Commit((Summry + "\n\n" + Description).Trim()))
                .AddTo(MultipleDisposable);
        }
    }
}