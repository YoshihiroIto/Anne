using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Anne.Foundation;
using Anne.Foundation.Extentions;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel.EventListeners;

namespace Anne.Model.Git
{
    public class Repository : ModelBase
    {
        // ブランチ
        public ReadOnlyReactiveCollection<Branch> Branches { get; }

        // コミット
        private ObservableCollection<Commit> _commits;

        public ObservableCollection<Commit> Commits
        {
            get { return _commits; }
            set
            {
                _commits?.ForEach(x => x.Dispose());
                SetProperty(ref _commits, value);
            }
        }

        // ジョブキュー
        public ReactiveCollection<string> JobSummaries => _jobQueue.JobSummaries;
        public ReadOnlyReactiveProperty<string> WorkingJob { get; private set; }
        public event EventHandler<ExceptionEventArgs> JobExecutingException;

        // ファイルステータス
        public FileStatus FileStatus { get; }

        //
        public string Path { get; }

        // 内部状態
        internal LibGit2Sharp.Repository Internal { get; }
        private readonly JobQueue _jobQueue = new JobQueue();

        private readonly ReactiveCollection<LibGit2Sharp.Branch> _branchesPool
            = new ReactiveCollection<LibGit2Sharp.Branch>(Scheduler.Immediate);

        public Repository(string path)
        {
            Path = path;
            Internal = new LibGit2Sharp.Repository(path).AddTo(MultipleDisposable);

            MultipleDisposable.Add(() => Commits.ForEach(x => x.Dispose()));

            // ジョブキュー
            _jobQueue.AddTo(MultipleDisposable);
            WorkingJob = _jobQueue.WorkingJob
                .ToReadOnlyReactiveProperty(eventScheduler: Scheduler.Immediate)
                .AddTo(MultipleDisposable);

            // ファイルステータス
            FileStatus = new FileStatus(this)
                .AddTo(MultipleDisposable);

            Observable.FromEventPattern<ExceptionEventArgs>(_jobQueue, nameof(JobQueue.JobExecutingException))
                .Select(x => x.EventArgs)
                .Subscribe(e => JobExecutingException?.Invoke(this, e))
                .AddTo(MultipleDisposable);

            Branches = _branchesPool
                .ToReadOnlyReactiveCollection(x => new Branch(x.CanonicalName, Internal), Scheduler.Immediate)
                .AddTo(MultipleDisposable);

            UpdateAll();

            {
                var watcher = new FileWatcher(System.IO.Path.Combine(Path, @".git\refs"), true)
                    .AddTo(MultipleDisposable);

                new EventListener<FileSystemEventHandler>(
                    h => watcher.FileUpdated += h,
                    h => watcher.FileUpdated -= h,
                    (s, e) => _jobQueue.AddJob("UpdateAll", UpdateAll))
                    .AddTo(MultipleDisposable);

                watcher.Start();
            }

            {
                var watcher = new FileWatcher(System.IO.Path.Combine(Path, @".git\HEAD"), false)
                    .AddTo(MultipleDisposable);

                new EventListener<FileSystemEventHandler>(
                    h => watcher.FileUpdated += h,
                    h => watcher.FileUpdated -= h,
                    (s, e) => UpdateBranchProps(Branches.ToArray()))
                    .AddTo(MultipleDisposable);

                watcher.Start();
            }

            MultipleDisposable.Add(() =>
                _commitLabelDict.Values
                    .SelectMany(x => x)
                    .ForEach(x => x.Dispose())
                );
        }

        public LibGit2Sharp.Commit FindCommitBySha(string commitSha)
        {
            var commit = Internal.Lookup<LibGit2Sharp.Commit>(commitSha);
            Debug.Assert(commit != null);

            return commit;
        }

        private void UpdateAll()
        {
            UpdateBranches();
            UpdateCommitLabelDict(Branches.ToArray());
            UpdateCommits();
            UpdateGraph();
        }

        private void UpdateBranches()
        {
            _branchesPool
                .Where(x => Internal.Branches.Any(y => y.CanonicalName == x.CanonicalName) == false)
                .ToArray()
                .ForEach(x => _branchesPool.Remove(x));

            Internal.Branches
                .Where(x => _branchesPool.Any(y => y.CanonicalName == x.CanonicalName) == false)
                .ToArray()
                .ForEach(x =>
                {
                    var insertIndex = _branchesPool.MakeInsertIndex(
                        x,
                        (l, r) => Comparer<string>.Default.Compare(l.CanonicalName, r.CanonicalName));

                    _branchesPool.Insert(insertIndex, x);
                });
        }

        private readonly Dictionary<string, Commit> _shaToCommit = new Dictionary<string, Commit>();

        private void UpdateCommits()
        {
            var index = 0;
            _shaToCommit.Clear();

            Commits =
                Internal.Commits.QueryBy(
                    new CommitFilter
                    {
                        SortBy = CommitSortStrategies.Time,
                        IncludeReachableFrom = Internal.Refs
                    })
                    .Take(App.MaxCommitCount)
                    .Select(x =>
                    {
                        var commit = new Commit(this, x.Sha, index++);
                        _shaToCommit.Add(commit.Sha, commit);
                        return commit;
                    })
                    .ToObservableCollection();
        }

        private void UpdateGraph()
        {
            var first = Commits.FirstOrDefault();
            if (first != null)
                first.CommitGraphNode.Current = 0;

            Commits.ForEach(commit =>
            {
#if true
                if (commit.Sha == "6b8fe7fb69c7106ed1622eecbf217ee7a1c07162")
                {
                    int a = 0;
                }
#endif

                commit.ParentShas.ForEach(parentSha =>
                {
                    Debug.Assert(_shaToCommit.ContainsKey(parentSha));
                    var parentIndex = _shaToCommit[parentSha].Index;

                    Commits[parentIndex].CommitGraphNode.PutCurrent();

                    // 自分の直後から親の直前まで
                    for (var i = commit.Index + 1; i < parentIndex; ++ i)
                        Commits[i].CommitGraphNode.Forward();
                });
            });
        }

        private void UpdateBranchProps(Branch[] branches)
        {
            branches.ForEach(x => x.UpdateProps());
        }

        private Dictionary<string /*commitSha*/, List<CommitLabel> /*label*/> _commitLabelDict =
            new Dictionary<string, List<CommitLabel>>();

        private void UpdateCommitLabelDict(Branch[] branches)
        {
            _commitLabelDict.Values
                .SelectMany(x => x)
                .ForEach(x => x.Dispose());

            _commitLabelDict = new Dictionary<string, List<CommitLabel>>();

            branches.ForEach(b =>
            {
                if (_commitLabelDict.ContainsKey(b.TipSha) == false)
                    _commitLabelDict[b.TipSha] = new List<CommitLabel>();

                _commitLabelDict[b.TipSha].Add(
                    new CommitLabel
                    {
                        Name = b.Name,
                        Type = b.IsRemote ? CommitLabelType.RemoveBranch : CommitLabelType.LocalBranch
                    });
            });
        }

        public IEnumerable<CommitLabel> GetCommitLabels(string commitSha)
        {
            List<CommitLabel> labels;

            return
                _commitLabelDict.TryGetValue(commitSha, out labels)
                    ? labels
                    : Enumerable.Empty<CommitLabel>();
        }

        public void Fetch(string remoteName)
        {
            _jobQueue.AddJob(
                $"Fetch: {remoteName}",
                () =>
                {
                    var remote = Internal.Network.Remotes[remoteName];

                    Internal.Config.Set("fetch.prune", true);
                    Internal.Network.Fetch(remote);
                });
        }

        public void FetchAll()
        {
            Internal.Network.Remotes.Select(r => r.Name).ForEach(Fetch);
        }

        public void Stage(params string[] paths)
        {
            _jobQueue.AddJob(
                $"Stage: {string.Join(",", paths)}",
                () => Internal.Stage(paths));
        }

        public void Unstage(params string[] paths)
        {
            _jobQueue.AddJob(
                $"Unstage: {string.Join(",", paths)}",
                () => Internal.Unstage(paths));
        }

        public void Commit(string message)
        {
            _jobQueue.AddJob(
                $"Commit: {message}",
                () =>
                {
                    var author = Internal.Config.BuildSignature(DateTimeOffset.Now);
                    Internal.Commit(message, author, author);
                });
        }

        public void DiscardChanges(IEnumerable<string> paths)
        {
            var enumerable = paths as string[] ?? paths.ToArray();
            _jobQueue.AddJob(
                $"DiscardChanges: {string.Join(",", enumerable)}",
                () =>
                {
                    var opts = new CheckoutOptions {CheckoutModifiers = CheckoutModifiers.Force};
                    Internal.CheckoutPaths("HEAD", enumerable, opts);
                });
        }

        public void Revert(string commitSha)
        {
            _jobQueue.AddJob(
                $"Revert: {commitSha}",
                () =>
                {
                    var commit = FindCommitBySha(commitSha);
                    var author = Internal.Config.BuildSignature(DateTimeOffset.Now);

                    Internal.Revert(commit, author);
                });
        }

        public void Reset(ResetMode mode, string commitSha)
        {
            _jobQueue.AddJob(
                $"Reset: {mode} {commitSha}",
                () =>
                {
                    var commit = FindCommitBySha(commitSha);
                    Internal.Reset(mode, commit);
                });
        }

        public void SwitchBranch(string branchName)
        {
            var branches = Branches.ToArray();

            _jobQueue.AddJob(
                $"SwitchBanch: {branchName}",
                () =>
                {
                    var branch = Branches.ToArray().FirstOrDefault(b => b.Name == branchName);
                    branch?.Switch();
                    UpdateBranchProps(branches);
                });
        }

        public void CheckoutBranch(string branchName)
        {
            var branches = Branches.ToArray();

            _jobQueue.AddJob(
                $"CheckoutBranch: {branchName}",
                () =>
                {
                    var srcBranch = Branches.ToArray().FirstOrDefault(b => b.Name == branchName);
                    srcBranch?.Checkout();
                    UpdateBranchProps(branches);
                });
        }

        public void RemoveBranches(IEnumerable<string> branchCanonicalNames)
        {
            var names = branchCanonicalNames.ToArray();

            _jobQueue.AddJob(
                "RemoveBranches",
                () => names.ForEach(x => Branches.ToArray().FirstOrDefault(b => b.CanonicalName == x)?.Remove()));
        }

        public void Pull()
        {
            Debug.WriteLine("Pull() -- 未実装");
        }

        public void Push()
        {
            _jobQueue.AddJob(
                "Push",
                () =>
                {
                    var currentBrunch = Internal.Branches.FirstOrDefault(x => x.IsCurrentRepositoryHead);
                    if (currentBrunch == null)
                        return;

                    Internal.Network.Push(currentBrunch);
                });
        }

        public void StartJob()
        {
            _jobQueue.Start();
        }

        public void AddJob(string summary, Action action)
        {
            _jobQueue.AddJob(summary, action);
        }
    }
}