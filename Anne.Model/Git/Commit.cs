using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anne.Foundation;
using Anne.Foundation.CommitGraph;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;

namespace Anne.Model.Git
{
    //[DebuggerDisplay("Current={CommitGraphNode.Current}, Depth={CommitGraphNode._depth}, Message={MessageShort}")]
    public class Commit : ModelBase
    {
        public readonly int Index;

        public string Message
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _messageCache,
                    () => new SavingMemoryString(Internal.Message)).Value;
            }
        }

        public string MessageShort
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _messageShortCache,
                    () => new SavingMemoryString(Internal.MessageShort)).Value;
            }
        }

        public string Sha { get; }
        public IEnumerable<string> ParentShas => Internal.Parents.Select(x => x.Sha); 

        public string AutherName => Internal.Author.Name;
        public string AutherEmail => Internal.Author.Email;
        public DateTimeOffset When => Internal.Author.When;

        private SavingMemoryString _messageCache;
        private SavingMemoryString _messageShortCache;

        private ObservableCollection<ChangeFile> _changeFiles = new ObservableCollection<ChangeFile>();

        public ObservableCollection<ChangeFile> ChangeFiles
        {
            set { SetProperty(ref _changeFiles, value); }

            get
            {
                lock (_isFileDiffsMakeDoneSync)
                {
                    if (_isFileDiffsMakeDone)
                        return _changeFiles;

                    _isFileDiffsMakeDone = true;
                }

                IsChangeFilesBuilding = true;
                Task.Run(() =>
                {
                    FileDiffs.ForEach(x => ChangeFiles.Add(x));
                    IsChangeFilesBuilding = false;
                    _disposeResetEvent?.Set();
                });

                return _changeFiles;
            }
        }

        private bool _isChangeFilesBuilding;

        public bool IsChangeFilesBuilding
        {
            get { return _isChangeFilesBuilding; }
            set { SetProperty(ref _isChangeFilesBuilding, value); }
        }

        public CommitGraphNode CommitGraphNode { get; } = new CommitGraphNode();

        private LibGit2Sharp.Commit Internal => _repos.FindCommitBySha(Sha);

        private bool _isFileDiffsMakeDone;
        private readonly object _isFileDiffsMakeDoneSync = new object();

        private readonly Repository _repos;
        private ManualResetEventSlim _disposeResetEvent;

        public Commit(Repository repos, string commitSha, int index)
        {
            Debug.Assert(repos != null);
            Debug.Assert(commitSha != null);

            _repos = repos;
            Sha = commitSha;
            Index = index;

            MultipleDisposable.AddFirst(() =>
            {
                lock (_isFileDiffsMakeDoneSync)
                {
                    if (_isFileDiffsMakeDone)
                        _disposeResetEvent = new ManualResetEventSlim();
                }
            });

            MultipleDisposable.Add(() => _changeFiles?.ForEach(f => f.Dispose()));
        }

        private IEnumerable<ChangeFile> FileDiffs
        {
            get
            {
                // internalは都度作るのでここで一度作り使い回す
                var commit = Internal;

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var parent in commit.Parents)
                {
                    foreach (var diff in _repos.Internal.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                    {
                        yield return new ChangeFile(_repos, parent.Tree, commit.Tree)
                        {
                            Path = diff.Path,
                            Status = diff.Status,
                        };
                    }
                }
            }
        }
    }
}