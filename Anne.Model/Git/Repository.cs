using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
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
        internal LibGit2Sharp.Repository Internal { get; }
        private readonly JobQueue _jobQueue = new JobQueue();

        public Repository(string path)
        {
            Internal = new LibGit2Sharp.Repository(path).AddTo(MultipleDisposable);

            // ジョブキュー
            _jobQueue.AddTo(MultipleDisposable);
            JobSummries = _jobQueue.JobSummries;
            WorkingJob = _jobQueue.WorkingJob
                .ToReadOnlyReactiveProperty(eventScheduler:Scheduler.Immediate)
                .AddTo(MultipleDisposable);

            Observable.FromEventPattern<ExceptionEventArgs>(_jobQueue, nameof(JobQueue.JobExecutingException))
                .Select(x => x.EventArgs)
                .Subscribe(e => JobExecutingException?.Invoke(this, e))
                .AddTo(MultipleDisposable);

            Branches = Internal.Branches
                .ToReadOnlyReactiveCollection(
                    Internal.Branches.ToCollectionChanged<LibGit2Sharp.Branch>(),
                    x => new Branch(x, Internal),
                    Scheduler.Immediate)
                .AddTo(MultipleDisposable);

            Commits = new ReactiveProperty<IEnumerable<Commit>>(
                Scheduler.Immediate,
                Internal.Commits.Select(x => new Commit(this, x)).Memoize())
                .AddTo(MultipleDisposable);

            new AnonymousDisposable(() => Commits.Value.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);
        }

        private void UpdateBranchProps()
        {
            Branches.ForEach(x => x.UpdateProps());
        }

        public void Fetch(string remoteName)
        {
            _jobQueue.AddJob(
                $"Fetch: {remoteName}",
                () =>
                {
                    var remote = Internal.Network.Remotes[remoteName];
                    Internal.Network.Fetch(remote);
                });
        }

        public void FetchAll()
        {
            Internal.Network.Remotes.Select(r => r.Name).ForEach(Fetch);
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

        #endregion
    }
}