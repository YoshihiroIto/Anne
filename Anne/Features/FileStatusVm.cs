using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reactive.Linq;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;

namespace Anne.Features
{
    public class FileStatusVm : ViewModelBase
    {
        public ReadOnlyReactiveProperty<IEnumerable<ChangingFileVm>> ChangingFiles { get; }

        private readonly Repository _repos;

        public FileStatusVm(Repository repos)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            ChangingFiles = repos.FileStatus.ChangingFiles
                .Select(x => x.Select(y => new ChangingFileVm(repos, y)))
                .ToReadOnlyReactiveProperty();
        }
    }
}