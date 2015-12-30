using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;
using StatefulModel.EventListeners;

namespace Anne.Model.Git
{
    public class FileStatus : ModelBase
    {
        public ReactiveProperty<IEnumerable<WipFile>> WipFiles { get; } =
            new ReactiveProperty<IEnumerable<WipFile>>(Scheduler.Immediate, new WipFile[0]);

        private readonly Repository _repos;

        public FileStatus(Repository repos)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            new AnonymousDisposable(() => WipFiles.Value.ForEach(c => c.Dispose()))
                .AddTo(MultipleDisposable);

            UpdateChangingFiles(null);

            var watcher = new FileWatcher(repos.Path)
                .AddTo(MultipleDisposable);

            new EventListener<FileSystemEventHandler>(
                h => watcher.FileUpdated += h,
                h => watcher.FileUpdated -= h,
                (s, e) => UpdateChangingFiles(e))
                .AddTo(MultipleDisposable);

            watcher.Start();
        }

        private void UpdateChangingFiles(FileSystemEventArgs e)
        {
            if (e != null)
                Debug.WriteLine($"UpdateChangingFiles : {e.FullPath}, {e.Name}, {e.ChangeType}");

            var old = WipFiles.Value;

            _repos.AddJob("UpdateChangingFiles", () =>
            {
                WipFiles.Value =
                    _repos.Internal.RetrieveStatus(new StatusOptions())
                        .Where(i => i.State != LibGit2Sharp.FileStatus.Ignored)
                        .Select(x =>
                        {
                            var compare = _repos.Internal.Diff.Compare<Patch>(
                                _repos.Internal.Head.Tip.Tree,
                                DiffTargets.Index | DiffTargets.WorkingDirectory,
                                new[] {x.FilePath}
                                ).FirstOrDefault();

                            return new WipFile(_repos, x.FilePath, IsInStaging(x.State))
                            {
                                Patch = compare?.Patch
                            };
                        })
                        .ToObservableCollection();

                old.ForEach(c => c.Dispose());
            });
        }

        private static bool IsInStaging(LibGit2Sharp.FileStatus status)
        {
            return IsInStagings.Any(i => (status & i) != 0);
        }

        private static readonly LibGit2Sharp.FileStatus[] IsInStagings =
        {
            LibGit2Sharp.FileStatus.NewInIndex,
            LibGit2Sharp.FileStatus.DeletedFromIndex,
            LibGit2Sharp.FileStatus.ModifiedInIndex,
            LibGit2Sharp.FileStatus.RenamedInIndex,
            LibGit2Sharp.FileStatus.TypeChangeInIndex
        };
    }
}