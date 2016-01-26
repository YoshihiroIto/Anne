using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
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
using Repository = Anne.Model.Git.Repository;

namespace Anne.Features
{
    public class RepositoryVm : ViewModelBase
    {
        public ReadOnlyReactiveCollection<string> JobSummaries { get; private set; }
        public ReadOnlyReactiveProperty<string> WorkingJob { get; private set; }

        public ReactiveProperty<ObservableCollection<ICommitVm>> Commits { get; }

        public ReadOnlyObservableCollection<BranchVm> LocalBranches { get; private set; }
        public ReadOnlyObservableCollection<BranchVm> RemoteBranches { get; private set; }

        public ReactiveProperty<ICommitVm> SelectedCommit { get; }
        public ReadOnlyReactiveProperty<ICommitVm> SelectedCommitDelay { get; }

        public ReactiveProperty<BranchVm> SelectedLocalBranch { get; }
        public ReactiveProperty<BranchVm> SelectedRemoteBranch { get; }

        public ReactiveProperty<WordFilter> WordFilter { get; }
        public ReactiveProperty<bool> IsFiltering { get; } 

        public ReactiveProperty<RepositoryOutlinerVm> Outliner { get; }

        public ReactiveCommand FetchCommand { get; } = new ReactiveCommand();
        public ReactiveCommand PushCommand { get; } = new ReactiveCommand();
        public ReactiveCommand PullCommand { get; } = new ReactiveCommand();

        public FileStatusVm FileStatus { get; }

        public string Path => _model.Path;
        public string Name => System.IO.Path.GetFileName(Path);

        private readonly Repository _model;

        private TwoPaneLayoutVm _twoPaneLayout;

        public TwoPaneLayoutVm TwoPaneLayout
        {
            get
            {
                if (_twoPaneLayout != null)
                    return _twoPaneLayout;
                return _twoPaneLayout = new TwoPaneLayoutVm().AddTo(MultipleDisposable);
            }
        }

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

            JobSummaries = _model.JobSummaries
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

            WordFilter = new ReactiveProperty<WordFilter>(new WordFilter()).AddTo(MultipleDisposable);
            IsFiltering = new ReactiveProperty<bool>().AddTo(MultipleDisposable);

            // コミット
            var observeCommits = _model.ObserveProperty(x => x.Commits);
            Commits = FileStatus.WipFiles.CombineLatest(
                observeCommits,
                WordFilter,
                (wipFiles, commits, wordFilter) => new {wipFiles, commits, wordFilter})
                .Do(_ => IsFiltering.Value = true)
                .Do(_ => _oldCommits.Enqueue(Commits?.Value))
                .Select(x =>
                {
                    var allCommits = new ObservableCollection<ICommitVm>();
                    {
                        if (x.wipFiles.Any())
                            allCommits.Add(new WipCommitVm(this, TwoPaneLayout));

                        x.commits
                            .AsParallel()
                            .AsOrdered()
                            .Where(y => x.wordFilter.IsMatch(y.Message))
                            .Select(y => (ICommitVm) new DoneCommitVm(this, y, TwoPaneLayout))
                            .ForEach(y => allCommits.Add(y));
                    }

                    return allCommits;
                })
                .Do(_ => IsFiltering.Value = false)
                .ToReactiveProperty()
                .AddTo(MultipleDisposable);

            MultipleDisposable.Add(() => Commits?.Value?.OfType<IDisposable>().ForEach(x => x.Dispose()));

            // 選択アイテム
            SelectedLocalBranch = new ReactiveProperty<BranchVm>().AddTo(MultipleDisposable);
            SelectedRemoteBranch = new ReactiveProperty<BranchVm>().AddTo(MultipleDisposable);

            SelectedCommit = Commits
                .Select(c => c?.FirstOrDefault())
                .ToReactiveProperty()
                .AddTo(MultipleDisposable);

            SelectedCommitDelay = SelectedCommit
                .Sample(TimeSpan.FromMilliseconds(150))
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            Commits
                .Delay(TimeSpan.FromMilliseconds(1000))
                .Subscribe(_ =>
                {
                    ObservableCollection<ICommitVm> oldCommits;
                    var i = _oldCommits.TryDequeue(out oldCommits);
                    Debug.Assert(i);

                    oldCommits?.OfType<IDisposable>().ForEach(x => x.Dispose());
                }).AddTo(MultipleDisposable);

            Observable.FromEventPattern<ExceptionEventArgs>(_model, nameof(_model.JobExecutingException))
                .Select(x => x.EventArgs)
                .ObserveOnUIDispatcher()
                .Subscribe(e => ShowDialog(e.Exception, e.Summary))
                .AddTo(MultipleDisposable);

            InitializeCommands();
        }

        private readonly ConcurrentQueue<ObservableCollection<ICommitVm>> _oldCommits
            = new ConcurrentQueue<ObservableCollection<ICommitVm>>();

        public void Commit(string message) => _model.Commit(message);
        public void DiscardChanges(IEnumerable<string> paths) => _model.DiscardChanges(paths);
        public void Reset(ResetMode mode, string commitSha) => _model.Reset(mode, commitSha);
        public void Revert(string commitSha) => _model.Revert(commitSha);
        public void SwitchBranch(string branchName) => _model.SwitchBranch(branchName);
        public void CheckoutBranch(string branchName) => _model.CheckoutBranch(branchName);

        public void RemoveBranches(IEnumerable<string> branchCanonicalNames)
            => _model.RemoveBranches(branchCanonicalNames);

        public IEnumerable<CommitLabel> GetCommitLabels(string commitSha)
        {
            return _model.GetCommitLabels(commitSha);
        }

        private void ShowDialog(Exception e, string summary)
        {
            Debug.Assert(e != null);

            var message = string.IsNullOrEmpty(summary) ? e.Message : $"{summary}\n--\n{e.Message}";

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