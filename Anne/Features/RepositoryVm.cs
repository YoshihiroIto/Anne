using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Anne.Features.Interfaces;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Anne.Windows;
using LibGit2Sharp;
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

        public ReactiveProperty<RepositoryOutlinerVm> Outliner { get; }

        public ReactiveCommand FetchCommand { get; } = new ReactiveCommand();
        public ReactiveCommand PushCommand { get; } = new ReactiveCommand();
        public ReactiveCommand PullCommand { get; } = new ReactiveCommand();

        public FileStatusVm FileStatus { get; }

        public string Path => _model.Path;
        public string Name => System.IO.Path.GetFileName(Path);

        private readonly Repository _model;

        // ※.管理は親側で行う
        public ReadOnlyReactiveCollection<RepositoryVm> Repositories => _parent.Repositories;
        public ReactiveProperty<RepositoryVm> SelectedRepository => _parent.SelectedRepository;

        private readonly MainWindowVm _parent;

        public RepositoryVm(Repository model, MainWindowVm parent)
        {
            Debug.Assert(model != null);
            Debug.Assert(parent != null);

            _model = model;
            _parent = parent;

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
                .ToReadOnlyReactiveCollection(x => new BranchVm(this, x))
                .AddTo(MultipleDisposable);

            RemoteBranches = _model.Branches
                .ToReadOnlyReactiveCollection()
                .ToFilteredReadOnlyObservableCollection(x => x.IsRemote)
                .ToReadOnlyReactiveCollection(x => new BranchVm(this, x))
                .AddTo(MultipleDisposable);

            // アウトライナー
            Outliner = new ReactiveProperty<RepositoryOutlinerVm>(new RepositoryOutlinerVm(this))
                .AddTo(MultipleDisposable);
            MultipleDisposable.Add(() => Outliner.Value.Dispose());

            // ファイルステータス
            FileStatus = new FileStatusVm(model)
                .AddTo(MultipleDisposable);

            // コミット
            Commits = new ReactiveCollection<ICommitVm>()
                .AddTo(MultipleDisposable);

            _model.Commits.Subscribe(src =>
            {
                var old = Commits.ToArray();

                Commits.AddRangeOnScheduler(src.Select(x => new DoneCommitVm(this, x)));

                old.ForEach(o =>
                {
                    Commits.RemoveOnScheduler(o);
                    (o as IDisposable)?.Dispose();
                });
            }).AddTo(MultipleDisposable);

            MultipleDisposable.Add(() => Commits?.OfType<IDisposable>().ForEach(x => x.Dispose()));

            // todo:_model.Commits と FileStatus.WipFiles をマージする
            FileStatus.WipFiles.Subscribe(wipFileVms =>
            {
                var frontItem = Commits.FirstOrDefault();

                if (wipFileVms.Any())
                {
                    if (frontItem == null || frontItem is DoneCommitVm)
                        Commits.InsertOnScheduler(0, new WipCommitVm(this));
                }
                else
                {
                    var vm = frontItem as WipCommitVm;
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
                .Subscribe(e => ShowDialog(e.Exception, e.Summry))
                .AddTo(MultipleDisposable);

            InitializeCommands();
        }

        public void Commit(string message) => _model.Commit(message);
        public void DiscardChanges(IEnumerable<string> paths) => _model.DiscardChanges(paths);
        public void Reset(ResetMode mode, string commitSha) => _model.Reset(mode, commitSha);
        public void Revert(string commitSha) => _model.Revert(commitSha);
        public void SwitchBranch(string branchName) => _model.SwitchBranch(branchName);
        public void CheckoutBranch(string branchName) => _model.CheckoutBranch(branchName);
        public void RemoveBranches(IEnumerable<string> branchCanonicalNames) => _model.RemoveBranches(branchCanonicalNames);

        public IEnumerable<CommitLabel> GetCommitLabels(string commitSha)
        {
            return _model.GetCommitLabels(commitSha);
        }

        private void ShowDialog(Exception e, string summry)
        {
            Debug.Assert(e != null);

            var message = string.IsNullOrEmpty(summry) ? e.Message : $"{summry}\n--\n{e.Message}}}";

            Messenger.Raise(new InformationMessage(message, "Information", "Info"));
        }

        private void InitializeCommands()
        {
            FetchCommand.AddTo(MultipleDisposable);
            PushCommand.AddTo(MultipleDisposable);
            PullCommand.AddTo(MultipleDisposable);

            FetchCommand.Subscribe(_ => _model.FetchAll()).AddTo(MultipleDisposable);
            PushCommand.Subscribe(_ => _model.Push()).AddTo(MultipleDisposable);
            PullCommand.Subscribe(_ => _model.Pull()).AddTo(MultipleDisposable);
        }
    }
}