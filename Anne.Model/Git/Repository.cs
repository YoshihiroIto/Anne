﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;

namespace Anne.Model.Git
{
    public class Repository : ModelBase
    {
        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();

        public ReadOnlyReactiveCollection<Branch> AllBranches { get; private set; }

        public IFilteredReadOnlyObservableCollection<Branch> LocalBranches { get; private set; }
        public IFilteredReadOnlyObservableCollection<Branch> RemoteBranches { get; private set; }

        private readonly ReadOnlyReactiveProperty<LibGit2Sharp.Repository> _repos;

        public Repository()
        {
            Path
                .AddTo(MultipleDisposable);

            _repos = Path
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => new LibGit2Sharp.Repository(path))
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            _repos
                .Where(r => r != null)
                .Select(r => r.Branches)
                .Subscribe(branches =>
                {
                    MultipleDisposable.RemoveAndDispose(AllBranches);
                    MultipleDisposable.RemoveAndDispose(LocalBranches);
                    MultipleDisposable.RemoveAndDispose(RemoteBranches);

                    AllBranches = branches
                        .ToReadOnlyReactiveCollection(
                            branches.ToCollectionChanged<LibGit2Sharp.Branch>(),
                            x => new Branch(x, _repos.Value)
                        ).AddTo(MultipleDisposable);


                    AllBranches
                        .ObserveAddChanged()
                        .Subscribe(b => b.UpdateProps())
                        .AddTo(MultipleDisposable);

                    LocalBranches = AllBranches.ToFilteredReadOnlyObservableCollection(x => ! x.IsRemote.Value);
                    RemoteBranches = AllBranches.ToFilteredReadOnlyObservableCollection(x => x.IsRemote.Value);
                }).AddTo(MultipleDisposable);
        }

        public async Task CheckoutTest()
        {
            var srcBranch = RemoteBranches.FirstOrDefault(b => b.Name.Value == "origin/refactoring");

            if (srcBranch != null)
                await srcBranch.CheckoutAsync();

            AllBranches.ForEach(x => x.UpdateProps());
        }

        public async Task RemoveTest()
        {
            var srcBranch = LocalBranches.FirstOrDefault(b => b.Name.Value == "refactoring");

            if (srcBranch != null)
                await srcBranch.RemoveAsync();

            AllBranches.ForEach(x => x.UpdateProps());
        }

        public async Task SwitchTest(string branchName)
        {
            var branch = LocalBranches.FirstOrDefault(b => b.Name.Value == branchName);
            
            if (branch != null)
                await branch.SwitchAsync();

            AllBranches.ForEach(x => x.UpdateProps());
        }
    }
}
