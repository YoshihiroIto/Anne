using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Anne.Branch;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;

namespace Anne.MainWindow
{
    public class MainWindowVm : ViewModelBase
    {
        public Repository Repository { get; }

        public ReadOnlyReactiveCollection<string> JobSummries { get; private set; }
        public ReadOnlyReactiveProperty<string> WorkingJob { get; private set; }

        public ReadOnlyReactiveProperty<IEnumerable<Commit>> Commits { get; private set; }

        public ReadOnlyObservableCollection<BranchVm> LocalBranches { get; private set; }
        public ReadOnlyObservableCollection<BranchVm> RemoteBranches { get; private set; }

        public MainWindowVm()
        {
            Repository = new Repository(@"C:\Users\yoi\Documents\Wox")
                .AddTo(MultipleDisposable);

            JobSummries = Repository.JobSummries
                .ToReadOnlyReactiveCollection()
                .AddTo(MultipleDisposable);

            WorkingJob = Repository.WorkingJob
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            Commits = Repository.Commits
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            LocalBranches = Repository.Branches
                .ToReadOnlyReactiveCollection()
                .ToFilteredReadOnlyObservableCollection(x => !x.IsRemote)
                .ToReadOnlyReactiveCollection(x => new BranchVm(x))
                .AddTo(MultipleDisposable);

            RemoteBranches = Repository.Branches
                .ToReadOnlyReactiveCollection()
                .ToFilteredReadOnlyObservableCollection(x => x.IsRemote)
                .ToReadOnlyReactiveCollection(x => new BranchVm(x))
                .AddTo(MultipleDisposable);
        }

        public void CheckoutTest()
        {
            Task.Run(() => Repository.CheckoutTest());
        }

        public void RemoveTest()
        {
            Task.Run(() => Repository.RemoveTest());
        }

        public void SwitchTest(string branchName)
        {
            Task.Run(() => Repository.SwitchTest(branchName));
        }

        public void FetchTest(string remoteName)
        {
            Task.Run(() => Repository.FetchTest(remoteName));
        }
    }
}
