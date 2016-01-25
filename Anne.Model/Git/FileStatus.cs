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

            MultipleDisposable.Add(() => WipFiles.Value.ForEach(c => c.Dispose()));

            UpdateWipFiles(null);

            var watcher = new FileWatcher(repos.Path, true)
                .AddTo(MultipleDisposable);

            new EventListener<FileSystemEventHandler>(
                h => watcher.FileUpdated += h,
                h => watcher.FileUpdated -= h,
                (s, e) => UpdateWipFiles(e))
                .AddTo(MultipleDisposable);

            watcher.Start();
        }

        private void UpdateWipFiles(FileSystemEventArgs e)
        {
            if (e != null)
                Debug.WriteLine($"UpdateWipFiles : {e.FullPath}, {e.Name}, {e.ChangeType}");

            var oldFiles = WipFiles.Value.ToArray();

            _repos.AddJob("UpdateWipFiles", () =>
            {
                var newFiles =
                    _repos.Internal.RetrieveStatus(new StatusOptions())
                        .Where(i => i.State != LibGit2Sharp.FileStatus.Ignored)
                        .Select(x =>
                        {
                            try
                            {
                                var compare =
                                    _repos.Internal.Diff.Compare<Patch>(
                                        _repos.Internal.Head.Tip.Tree,
                                        DiffTargets.Index | DiffTargets.WorkingDirectory,
                                        new[] {x.FilePath}
                                        ).FirstOrDefault();

                                if (compare != null)
                                {
                                    return
                                        new WipFile(_repos, IsInStaging(x.State), x.FilePath)
                                        {
                                            Patch = compare.Patch,
                                            LinesAdded = compare.LinesAdded,
                                            LinesDeleted = compare.LinesDeleted,
                                            Status = compare.Status,
                                            IsBinary = compare.IsBinaryComparison
                                        };
                                }
                            }
                            catch
                            {
                                // ignored
                            }

                            return null;
                        })
                        .Where(x => x != null)
                        .ToObservableCollection();

                if (newFiles.Select(x => x.Path).SequenceEqual(oldFiles.Select(x => x.Path)))
                {
                    oldFiles.Zip(newFiles, (o, n) => new {Old = o, New = n})
                        .ForEach(x => x.Old.CopyFrom(x.New));

                    newFiles.ForEach(c => c.Dispose());
                }
                else
                {
                    WipFiles.Value = newFiles;
                    oldFiles.ForEach(c => c.Dispose());
                }
            });
        }

        private static bool IsInStaging(LibGit2Sharp.FileStatus status)
        {
            return IsInStagings.Any(i => (status & i) != 0);
        }

        private static readonly LibGit2Sharp.FileStatus[] IsInStagings =
        {
            LibGit2Sharp.FileStatus.ModifiedInIndex,
        };
    }
}