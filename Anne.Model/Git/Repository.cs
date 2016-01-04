using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;
using StatefulModel.EventListeners;

namespace Anne.Model.Git
{
    public class Repository : ModelBase
    {
        // ブランチ
        public ReadOnlyReactiveCollection<Branch> Branches { get; }

        // コミット
        public ReactiveProperty<IEnumerable<Commit>> Commits { get; }

        // ジョブキュー
        public ReactiveCollection<string> JobSummries => _jobQueue.JobSummries;
        public ReadOnlyReactiveProperty<string> WorkingJob { get; private set; }
        public event EventHandler<ExceptionEventArgs> JobExecutingException;

        // ファイルステータス
        public FileStatus FileStatus { get; }

        //
        public string Path { get; }

        // 内部状態
        internal LibGit2Sharp.Repository Internal { get; }
        private readonly JobQueue _jobQueue = new JobQueue();

        public Repository(string path)
        {
            Path = path;
            Internal = new LibGit2Sharp.Repository(path).AddTo(MultipleDisposable);

            // ファイルステータス
            FileStatus = new FileStatus(this)
                .AddTo(MultipleDisposable);

            // ジョブキュー
            _jobQueue.AddTo(MultipleDisposable);
            WorkingJob = _jobQueue.WorkingJob
                .ToReadOnlyReactiveProperty(eventScheduler: Scheduler.Immediate)
                .AddTo(MultipleDisposable);

            Observable.FromEventPattern<ExceptionEventArgs>(_jobQueue, nameof(JobQueue.JobExecutingException))
                .Select(x => x.EventArgs)
                .Subscribe(e => JobExecutingException?.Invoke(this, e))
                .AddTo(MultipleDisposable);

            Branches = Internal.Branches
                .ToReadOnlyReactiveCollection(
                    Internal.Branches.ToCollectionChanged<LibGit2Sharp.Branch>(),
                    x => new Branch(x.CanonicalName, Internal),
                    Scheduler.Immediate)
                .AddTo(MultipleDisposable);

            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Time,
                IncludeReachableFrom = Internal.Refs
            };

            {
                UpdateCommitLabelDict();

                Commits = new ReactiveProperty<IEnumerable<Commit>>(
                    Scheduler.Immediate,
                    Internal.Commits.QueryBy(filter)
                        .Select(x => new Commit(this, x)).Memoize())
                    .AddTo(MultipleDisposable);

                new AnonymousDisposable(() => Commits.Value.ForEach(x => x.Dispose()))
                    .AddTo(MultipleDisposable);
            }

            {
                var watcher = new FileWatcher(System.IO.Path.Combine(Path, @".git\refs"))
                    .AddTo(MultipleDisposable);

                new EventListener<FileSystemEventHandler>(
                    h => watcher.FileUpdated += h,
                    h => watcher.FileUpdated -= h,
                    (s, e) =>
                    {
                        UpdateCommitLabelDict();

                        var old = Commits.Value;
                        Commits.Value = Internal.Commits.QueryBy(filter)
                            .Select(x => new Commit(this, x)).Memoize();
                        old.ForEach(x => x.Dispose());
                    })
                    .AddTo(MultipleDisposable);

                watcher.Start();
            }

            new AnonymousDisposable(() =>
                _commitLabelDict.Values
                    .SelectMany(x => x)
                    .ForEach(x => x.Dispose())
                ).AddTo(MultipleDisposable);
        }

        private void UpdateBranchProps()
        {
            Branches.ForEach(x => x.UpdateProps());
        }

        private Dictionary<string /*commitSha*/, List<CommitLabel> /*label*/> _commitLabelDict =
            new Dictionary<string, List<CommitLabel>>();

        private void UpdateCommitLabelDict()
        {
            _commitLabelDict.Values
                .SelectMany(x => x)
                .ForEach(x => x.Dispose());

            _commitLabelDict = new Dictionary<string, List<CommitLabel>>();

            Branches.ForEach(b =>
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
                    var commit = Internal.Lookup<LibGit2Sharp.Commit>(commitSha);
                    Debug.Assert(commit != null);

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
                    var commit = Internal.Lookup<LibGit2Sharp.Commit>(commitSha);
                    Debug.Assert(commit != null);
                    Internal.Reset(mode, commit);
                });
        }

        public void SwitchBranch(string branchName)
        {
            _jobQueue.AddJob(
                $"SwitchBanch: {branchName}",
                () =>
                {
                    var branch = Branches.FirstOrDefault(b => b.Name == branchName);
                    branch?.Switch();
                    UpdateBranchProps();
                });
        }

        public void CheckoutBranch(string branchName)
        {
            _jobQueue.AddJob(
                "CheckoutBranch",
                () =>
                {
                    var srcBranch = Branches.FirstOrDefault(b => b.Name == branchName);
                    srcBranch?.Checkout();
                    UpdateBranchProps();
                });
        }

        public void RemoveBranch(string branchName)
        {
            _jobQueue.AddJob(
                "RemoveBranch",
                () =>
                {
                    var srcBranch = Branches.FirstOrDefault(b => b.Name == branchName);
                    srcBranch?.Remove();
                });
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

        public void AddJob(string summry, Action action)
        {
            _jobQueue.AddJob(summry, action);
        }

        public void ExecuteJobSync(string summry, Action action)
        {
            using (var sema = new SemaphoreSlim(0, 1))
            {
                _jobQueue.AddJob(summry, () =>
                {
                    action();

                    // ReSharper disable once AccessToDisposedClosure
                    sema.Release();
                });

                sema.Wait();
            }
        }
    }
}