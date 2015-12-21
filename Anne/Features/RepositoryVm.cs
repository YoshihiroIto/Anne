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
        public ReactiveProperty<Commit> SelectedCommit { get; }

        public ReadOnlyObservableCollection<BranchVm> LocalBranches { get; private set; }
        public ReactiveProperty<BranchVm> SelectedLocalBranch { get; }

        public ReadOnlyObservableCollection<BranchVm> RemoteBranches { get; private set; }
        public ReactiveProperty<BranchVm> SelectedRemoteBranch { get; }

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

            // ブランチ
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

            // コミット
            Commits = _model.Commits
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            // 選択アイテム
            SelectedCommit = new ReactiveProperty<Commit>().AddTo(MultipleDisposable);
            SelectedLocalBranch = new ReactiveProperty<BranchVm>().AddTo(MultipleDisposable);
            SelectedRemoteBranch = new ReactiveProperty<BranchVm>().AddTo(MultipleDisposable);

            // test
            {
                SelectedCommit.Where(x => x != null)
                    .Subscribe(x => Debug.WriteLine(x.MessageShort))
                    .AddTo(MultipleDisposable);

                SelectedLocalBranch.Where(x => x != null)
                    .Subscribe(x => Debug.WriteLine(x.Name.Value))
                    .AddTo(MultipleDisposable);

                SelectedRemoteBranch.Where(x => x != null)
                    .Subscribe(x => Debug.WriteLine(x.Name.Value))
                    .AddTo(MultipleDisposable);
            }

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
            Task.Run(() => _model.Fetch(remoteName));
        }

        public void FetchAllTest()
        {
            Task.Run(() => _model.FetchAll());
        }
    }
}