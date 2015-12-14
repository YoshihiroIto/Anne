using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Model.Git
{
    public class Repository : ModelBase
    {
        // ブランチ
        public ReadOnlyReactiveCollection<Branch> Branches { get; }

        // コミット
        public ReactiveProperty<IEnumerable<Commit>> Commits { get; }

        // ジョブキュー
        public ReadOnlyReactiveCollection<string> JobSummries { get; private set; }
        public ReadOnlyReactiveProperty<string> WorkingJob { get; private set; }
        public event EventHandler<ExceptionEventArgs> JobExecutingException;

        // 内部状態
        private readonly LibGit2Sharp.Repository _internal;
        private readonly JobQueue _jobQueue = new JobQueue();

        public Repository(string path)
        {
            _internal = new LibGit2Sharp.Repository(path).AddTo(MultipleDisposable);

            // ジョブキュー
            MultipleDisposable.Add(_jobQueue);
            JobSummries = _jobQueue.JobSummries;
            WorkingJob = _jobQueue.WorkingJob.ToReadOnlyReactiveProperty().AddTo(MultipleDisposable);

            Observable.FromEventPattern<ExceptionEventArgs>(_jobQueue, nameof(JobQueue.JobExecutingException))
                .Select(x => x.EventArgs)
                .Subscribe(e => JobExecutingException?.Invoke(this, e))
                .AddTo(MultipleDisposable);

            Branches = _internal.Branches
                .ToReadOnlyReactiveCollection(
                    _internal.Branches.ToCollectionChanged<LibGit2Sharp.Branch>(),
                    x => new Branch(x, _internal),
                    Scheduler.Immediate)
                .AddTo(MultipleDisposable);

            Commits = new ReactiveProperty<IEnumerable<Commit>>(
                        Scheduler.Immediate,
                        _internal.Commits.Select(x => new Commit(x)).Memoize()
                    )
                    .AddTo(MultipleDisposable);

            new AnonymousDisposable(() => Commits.Value.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);
        }

        private void UpdateBranchProps()
        {
            Branches.ForEach(x => x.UpdateProps());
        }

#region Test

        public void CheckoutTest()
        {
            _jobQueue.AddJob(
                "Checkout",
                () =>
                {
                    var srcBranch = Branches.FirstOrDefault(b => b.Name == "origin/refactoring");
                    srcBranch?.Checkout();
                    UpdateBranchProps();
                });
        }

        public void RemoveTest()
        {
            _jobQueue.AddJob(
                "Remove",
                () =>
                {
                    var srcBranch = Branches.FirstOrDefault(b => b.Name == "refactoring");
                    srcBranch?.Remove();
                });
        }

        public void SwitchTest(string branchName)
        {
            _jobQueue.AddJob(
                $"Switch: {branchName}",
                () =>
                {
                    var branch = Branches.FirstOrDefault(b => b.Name == branchName);
                    branch?.Switch();
                    UpdateBranchProps();
                });
        }

        public void FetchTest(string remoteName)
        {
            _jobQueue.AddJob(
                $"Fetch: {remoteName}",
                () =>
                {
                    var remote = _internal.Network.Remotes[remoteName];
                    _internal.Network.Fetch(remote);
                });
        }

#endregion
    }
}