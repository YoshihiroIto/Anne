using System;
using System.Collections.ObjectModel;
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
        public ObservableCollection<Commit> Commits { get; private set; }

        public Repository()
        {
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
                    Commits?.ForEach(x => x.Dispose());

                    Commits = new ObservableCollection<Commit>(commits.Select(x => new Commit(x)));
                }).AddTo(MultipleDisposable);

            MultipleDisposable.Add(
                new AnonymousDisposable(
                    () => Commits?.ForEach(x => x.Dispose()))
                );
        }

        public void CheckoutTest()
        {
            var srcBranch = RemoteBranches.FirstOrDefault(b => b.Name.Value == "origin/refactoring");

            srcBranch?.Checkout();

            UpdateBranchProps();
        }

        public void RemoveTest()
        {
            var srcBranch = LocalBranches.FirstOrDefault(b => b.Name.Value == "refactoring");

            srcBranch?.Remove();
        }

        public void SwitchTest(string branchName)
        {
            var branch = LocalBranches.FirstOrDefault(b => b.Name.Value == branchName);

            branch?.Switch();

            UpdateBranchProps();
        }

        private void UpdateBranchProps()
        {
            Branches.ForEach(x => x.UpdateProps());
        }
    }
}