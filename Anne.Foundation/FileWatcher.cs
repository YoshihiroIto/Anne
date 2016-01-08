using System;
using System.IO;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Foundation
{
    public class FileWatcher : IDisposable
    {
        public event FileSystemEventHandler FileUpdated;

        private readonly MultipleDisposable _disposable = new MultipleDisposable();
        private readonly FileSystemWatcher _watcher;

        public FileWatcher(string path, bool isDirectory)
        {
            string dirname;
            string filename;

            if (isDirectory)
            {
                dirname = path;
                filename = string.Empty;
            }
            else
            {
                dirname = Path.GetDirectoryName(path) ?? string.Empty;
                filename = Path.GetFileName(path) ?? string.Empty;
            }

            _watcher = new FileSystemWatcher(dirname, filename)
            {
                IncludeSubdirectories = isDirectory,
                Filter = isDirectory ? "*.*" : string.Empty,
                NotifyFilter =
                    NotifyFilters.FileName |
                    (isDirectory ? NotifyFilters.DirectoryName : 0) |
                    NotifyFilters.LastWrite
            }.AddTo(_disposable);

            Observable.Merge(
                _watcher.CreatedAsObservable(),
                _watcher.DeletedAsObservable(),
                _watcher.ChangedAsObservable(),
                _watcher.RenamedAsObservable())
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(e => FileUpdated?.Invoke(this, e))
                .AddTo(_disposable);
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
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