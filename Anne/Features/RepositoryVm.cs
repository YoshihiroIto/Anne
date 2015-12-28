using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using Anne.Features.Interfaces;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Livet.Messaging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using StatefulModel;
using Repository = Anne.Model.Git.Repository;

namespace Anne.Features
{
    public class RepositoryVm : ViewModelBase
    {
        public ReadOnlyReactiveCollection<string> JobSummries { get; private set; }
        public ReadOnlyReactiveProperty<string> WorkingJob { get; private set; }

        public ReactiveCollection<ICommitVm> Commits { get; }
        public ReadOnlyObservableCollection<BranchVm> LocalBranches { get; private set; }
        public ReadOnlyObservableCollection<BranchVm> RemoteBranches { get; private set; }

        public ReactiveProperty<ICommitVm> SelectedCommit { get; }
        public ReactiveProperty<BranchVm> SelectedLocalBranch { get; }
        public ReactiveProperty<BranchVm> SelectedRemoteBranch { get; }

        public FileStatusVm FileStatus { get; }

        private readonly Repository _model;

        public RepositoryVm(Repository model)
        {
            Debug.Assert(model != null);
            _model = model;

            JobSummries = _model.JobSummries
                .ToReadOnlyReactiveCollection(UIDispatcherScheduler.Default)
                .AddTo(MultipleDisposable);

            WorkingJob = _model.WorkingJob
                .ToReadOnlyReactiveProperty(eventScheduler:UIDispatcherScheduler.Default)
                .AddTo(MultipleDisposable);

            // ブランチ
            LocalBranches = _model.Branches
                .ToReadOnlyReactiveCollection(UIDispatcherScheduler.Default)
                .ToFilteredReadOnlyObservableCollection(x => !x.IsRemote)
                .ToReadOnlyReactiveCollection(x => new BranchVm(x))
                .AddTo(MultipleDisposable);

            RemoteBranches = _model.Branches
                .ToReadOnlyReactiveCollection(UIDispatcherScheduler.Default)
                .ToFilteredReadOnlyObservableCollection(x => x.IsRemote)
                .ToReadOnlyReactiveCollection(x => new BranchVm(x))
                .AddTo(MultipleDisposable);

            // ファイルステータス
            FileStatus = new FileStatusVm(model)
                .AddTo(MultipleDisposable);

            // コミット
            Commits = new ReactiveCollection<ICommitVm>()
                .AddTo(MultipleDisposable);

            _model.Commits.Subscribe(src =>
            {
                Commits.OfType<IDisposable>().ForEach(x => x.Dispose());
                Commits.AddRangeOnScheduler(src.Select(x => new DoneCommitVm(x)));
            }).AddTo(MultipleDisposable);

            new AnonymousDisposable(() => Commits?.OfType<IDisposable>().ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);

            // todo:_model.Commits と FileStatus.ChangingFiles をマージする
            FileStatus.ChangingFiles.Subscribe(changeingFiles =>
            {
                var frontItem = Commits.FirstOrDefault();

                if (changeingFiles.Any())
                {
                    if (frontItem == null || frontItem is DoneCommitVm)
                        Commits.InsertOnScheduler(0, new WorkInProgressCommitVm(FileStatus));
                }
                else
                {
                    var vm = frontItem as WorkInProgressCommitVm;
                    if (vm != null)
                    {
                        Commits.RemoveAtOnScheduler(0);
                        vm.Dispose();
                    }
                }
            });

            // 選択アイテム
            SelectedCommit = new ReactiveProperty<ICommitVm>().AddTo(MultipleDisposable);
            SelectedLocalBranch = new ReactiveProperty<BranchVm>().AddTo(MultipleDisposable);
            SelectedRemoteBranch = new ReactiveProperty<BranchVm>().AddTo(MultipleDisposable);

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

        // test
        public void CheckoutTest() => _model.CheckoutTest();
        public void RemoveTest() => _model.RemoveTest();
        public void SwitchTest(string branchName) => _model.SwitchTest(branchName);
        public void FetchTest(string remoteName) => _model.Fetch(remoteName);
        public void FetchAllTest() => _model.FetchAll();
    }
}