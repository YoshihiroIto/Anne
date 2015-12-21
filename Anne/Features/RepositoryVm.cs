using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Anne.Model;
using Anne.Model.Git;
using Livet.Messaging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;

namespace Anne.Features
{
    public class RepositoryVm : ViewModelBase
    {
        public ReadOnlyReactiveCollection<string> JobSummries { get; private set; }
        public ReadOnlyReactiveProperty<string> WorkingJob { get; private set; }

        public ReadOnlyReactiveProperty<IEnumerable<Commit>> Commits { get; private set; }

        public ReadOnlyObservableCollection<BranchVm> LocalBranches { get; private set; }
        public ReadOnlyObservableCollection<BranchVm> RemoteBranches { get; private set; }

        private readonly Repository _model;

        public RepositoryVm(Repository model)
        {
            Debug.Assert(model != null);
            _model = model;

            JobSummries = _model.JobSummries
                .ToReadOnlyReactiveCollection()
                .AddTo(MultipleDisposable);

            WorkingJob = _model.WorkingJob
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            Commits = _model.Commits
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            LocalBranches = _model.Branches
                .ToReadOnlyReactiveCollection()
                .ToFilteredReadOnlyObservableCollection(x => !x.IsRemote)
                .ToReadOnlyReactiveCollection(x => new BranchVm(x))
                .AddTo(MultipleDisposable);

            RemoteBranches = _model.Branches
                .ToReadOnlyReactiveCollection()
                .ToFilteredReadOnlyObservableCollection(x => x.IsRemote)
                .ToReadOnlyReactiveCollection(x => new BranchVm(x))
                .AddTo(MultipleDisposable);

            Observable.FromEventPattern<ExceptionEventArgs>(_model, nameof(_model.JobExecutingException))
                .Select(x => x.EventArgs)
                .ObserveOnUIDispatcher()
                .Subscribe(e => ShowDialog(e.Exception))
                .AddTo(MultipleDisposable);
        }

        private void ShowDialog(Exception e)
        {
            Debug.Assert(e != null);

            Messenger.Raise(new InformationMessage(e.Message, "Information", "Info"));
        }

        public void CheckoutTest()
        {
            Task.Run(() => _model.CheckoutTest());
        }

        public void RemoveTest()
        {
            Task.Run(() => _model.RemoveTest());
        }

        public void SwitchTest(string branchName)
        {
            Task.Run(() => _model.SwitchTest(branchName));
        }

        public void FetchTest(string remoteName)
        {
            Task.Run(() => _model.FetchTest(remoteName));
        }
    }
}