using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reactive.Linq;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings;
using Repository = Anne.Model.Git.Repository;

namespace Anne.Features
{
    public class FileStatusVm : ViewModelBase
    {
        public ReadOnlyReactiveProperty<IEnumerable<WipFileVm>> WipFiles { get; }

        public FileStatusVm(Repository repos)
        {
            Debug.Assert(repos != null);

            WipFiles = repos.FileStatus.WipFiles
                .Select(x => x
                    .Where(z => z.Status != ChangeKind.Unmodified)
                    .Select(y => new WipFileVm(repos, y)))
                .ToReadOnlyReactiveProperty();
        }
    }
}