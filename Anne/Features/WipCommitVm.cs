using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
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

        public ReadOnlyReactiveProperty<IEnumerable<WipFileVm>> WipFiles
            => _fileStatus.WipFiles;

        public ReactiveProperty<WipFileVm> SelectedWipFile { get; }
            = new ReactiveProperty<WipFileVm>();

        public ReactiveCommand CommitCommand { get; } 

        private readonly FileStatusVm _fileStatus;
        private string _summry = string.Empty;
        private string _description = string.Empty;

        public WipCommitVm(FileStatusVm fileStatus)
        {
            Debug.Assert(fileStatus != null);
            _fileStatus = fileStatus;

            SelectedWipFile.AddTo(MultipleDisposable);

            CommitCommand = this.ObserveProperty(x => x.Summry)
                .Select(x => string.IsNullOrWhiteSpace(x) == false)
                .ToReactiveCommand()
                .AddTo(MultipleDisposable);

            CommitCommand.Subscribe(_ =>
            {
                Debug.WriteLine("Commit");
            }).AddTo(MultipleDisposable);
        }
    }
}