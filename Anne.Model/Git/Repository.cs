using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using StatefulModel;

namespace Anne.Model.Git
{
    public class Repository : ModelBase
    {
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();

        // ブランチ
        public ReadOnlyReactiveCollection<Branch> Branches { get; private set; }
        public IFilteredReadOnlyObservableCollection<Branch> LocalBranches { get; private set; }
        public IFilteredReadOnlyObservableCollection<Branch> RemoteBranches { get; private set; }

        // コミット
        public ReactiveProperty<List<Commit>> Commits { get; } = new ReactiveProperty<List<Commit>>();

        // 
        private readonly RepositoryJob _reposJob = new RepositoryJob();

        public Repository()
        {
            MultipleDisposable.Add(_reposJob);

            Path
                .AddTo(MultipleDisposable);

            var repos = Path
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => new LibGit2Sharp.Repository(path))
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            // ブランチ
            repos
                .Where(r => r != null)
                .Select(r => r.Branches)
                .Subscribe(branches =>
                {
                    MultipleDisposable.RemoveAndDispose(Branches);
                    MultipleDisposable.RemoveAndDispose(LocalBranches);
                    MultipleDisposable.RemoveAndDispose(RemoteBranches);

                    Branches = branches
                        .ToReadOnlyReactiveCollection(
                            branches.ToCollectionChanged<LibGit2Sharp.Branch>(),
                            x => new Branch(x, repos.Value)
                        ).AddTo(MultipleDisposable);

                    Branches
                        .ObserveAddChanged()
                        .Subscribe(b => b.UpdateProps())
                        .AddTo(MultipleDisposable);

                    LocalBranches = Branches.ToFilteredReadOnlyObservableCollection(x => !x.IsRemote.Value);
                    RemoteBranches = Branches.ToFilteredReadOnlyObservableCollection(x => x.IsRemote.Value);
                }).AddTo(MultipleDisposable);

            // コミット
            repos
                .Where(r => r != null)
                .Select(r => r.Commits)
                .Subscribe(commits =>
                {
                    Commits.Value?.ForEach(x => x.Dispose());
                    Commits.Value = commits.Select(x => new Commit(x)).ToList();
                }).AddTo(MultipleDisposable);

            MultipleDisposable.Add(new AnonymousDisposable(() =>
                Commits.Value?.ForEach(x => x.Dispose())
                ));
        }

        private void UpdateBranchProps()
        {
            Branches.ForEach(x => x.UpdateProps());
        }

        #region Test

        public void CheckoutTest()
        {
            _reposJob.AddJob(() =>
            {
                var srcBranch = RemoteBranches.FirstOrDefault(b => b.Name.Value == "origin/refactoring");
                srcBranch?.Checkout();
                UpdateBranchProps();
            });
        }

        public void RemoveTest()
        {
            _reposJob.AddJob(() =>
            {
                var srcBranch = LocalBranches.FirstOrDefault(b => b.Name.Value == "refactoring");
                srcBranch?.Remove();
            });
        }

        public void SwitchTest(string branchName)
        {
            _reposJob.AddJob(() =>
            {
                var branch = LocalBranches.FirstOrDefault(b => b.Name.Value == branchName);
                branch?.Switch();
                UpdateBranchProps();
            });
        }

        #endregion
    }
}