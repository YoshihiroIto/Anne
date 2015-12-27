using System.Collections.Generic;
using System.Diagnostics;
using Anne.Features.Interfaces;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class WorkInProgressCommitVm : ViewModelBase, ICommitVm
    {
        // ICommitVm
        public string Caption => "Uncommited changes";

        public ReadOnlyReactiveProperty<IEnumerable<ChangingFileVm>> ChangingFiles
            => _fileStatus.ChangingFiles;

        public ReactiveProperty<ChangingFileVm> SelectedChangingFile { get; }
            = new ReactiveProperty<ChangingFileVm>();

        private readonly FileStatusVm _fileStatus;

        public WorkInProgressCommitVm(FileStatusVm fileStatus)
        {
            Debug.Assert(fileStatus != null);
            _fileStatus = fileStatus;

            SelectedChangingFile.AddTo(MultipleDisposable);
        }
    }
}