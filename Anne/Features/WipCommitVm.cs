using System.Collections.Generic;
using System.Diagnostics;
using Anne.Features.Interfaces;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class WipCommitVm : ViewModelBase, ICommitVm
    {
        // ICommitVm
        public string Caption => "Uncommited changes";

        public ReadOnlyReactiveProperty<IEnumerable<WipFileVm>> WipFiles
            => _fileStatus.WipFiles;

        public ReactiveProperty<WipFileVm> SelectedWipFile { get; }
            = new ReactiveProperty<WipFileVm>();

        private readonly FileStatusVm _fileStatus;

        public WipCommitVm(FileStatusVm fileStatus)
        {
            Debug.Assert(fileStatus != null);
            _fileStatus = fileStatus;

            SelectedWipFile.AddTo(MultipleDisposable);
        }
    }
}