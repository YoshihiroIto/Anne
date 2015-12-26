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

        public FileStatusVm(FileStatus model)
        {
            Debug.Assert(model != null);

            ChangingFiles = model.ChangingFiles
                .Select(x =>
                    x.Select(y => new ChangingFileVm
                    {
                        Path = y.Path,
                        IsInStaging = y.IsInStaging
                    })
                )
                .ToReadOnlyReactiveProperty();
        }
    }
}