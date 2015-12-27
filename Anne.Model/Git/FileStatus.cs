using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Model.Git
{
    public class FileStatus : ModelBase
    {
        public ReactiveProperty<IEnumerable<ChangingFile>> ChangingFiles { get; } =
            new ReactiveProperty<IEnumerable<ChangingFile>>(Scheduler.Immediate, new ChangingFile[0]);

        private readonly Repository _repos;

        public FileStatus(Repository repos)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            new AnonymousDisposable(() => ChangingFiles.Value.ForEach(c => c.Dispose()))
                .AddTo(MultipleDisposable);

            UpdateChengingFiles(null);

            var watcher = new FileSystemWatcher(repos.Path)
            {
                Filter = string.Empty,
                IncludeSubdirectories = true,
                NotifyFilter =
                    NotifyFilters.FileName |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.LastWrite
            }.AddTo(MultipleDisposable);

            Observable.Merge(
                watcher.CreatedAsObservable(),
                watcher.DeletedAsObservable(),
                watcher.ChangedAsObservable(),
                watcher.RenamedAsObservable())
                .Throttle(TimeSpan.FromMilliseconds(500))
//                .Where(x => x.FullPath != Path.Combine(repos.Path, @".git"))
//                .Where(x => x.FullPath != Path.Combine(repos.Path, @".git\index.lock"))
//                .Where(x => x.FullPath != Path.Combine(repos.Path, @".git\index"))
                .Subscribe(UpdateChengingFiles)
                .AddTo(MultipleDisposable);

            watcher.EnableRaisingEvents = true;
        }

        private void UpdateChengingFiles(FileSystemEventArgs e)
        {
            if (e != null)
                Debug.WriteLine($"UpdateChengingFiles : {e.FullPath}, {e.Name}, {e.ChangeType}");

            var old = ChangingFiles.Value;

            _repos.AddJob("UpdateChengingFiles", () =>
            {
                ChangingFiles.Value =
                    _repos.Internal.RetrieveStatus(new LibGit2Sharp.StatusOptions())
                        .Where(i => i.State != LibGit2Sharp.FileStatus.Ignored)
                        .Select(x => new ChangingFile(_repos, x.FilePath, IsInStaging(x.State)))
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

    // http://blog.okazuki.jp/entry/20120322/1332378060
    internal static class FileSystemWatcherExtensions
    {
        public static IObservable<FileSystemEventArgs> ChangedAsObservable(this FileSystemWatcher self)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (_, e) => h(e),
                h => self.Changed += h,
                h => self.Changed -= h);
        }

        public static IObservable<FileSystemEventArgs> CreatedAsObservable(this FileSystemWatcher self)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (_, e) => h(e),
                h => self.Created += h,
                h => self.Created -= h);
        }

        public static IObservable<FileSystemEventArgs> DeletedAsObservable(this FileSystemWatcher self)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (_, e) => h(e),
                h => self.Deleted += h,
                h => self.Deleted -= h);
        }

        public static IObservable<FileSystemEventArgs> RenamedAsObservable(this FileSystemWatcher self)
        {
            return Observable.FromEvent<RenamedEventHandler, FileSystemEventArgs>(
                h => (_, e) => h(e),
                h => self.Renamed += h,
                h => self.Renamed -= h);
        }
    }
}