using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Anne.Features.Interfaces;
using Anne.Foundation;
using Anne.Foundation.Extentions;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class DoneCommitVm : ViewModelBase, ICommitVm
    {
        // ICommitVm
        public string Summary => MessageShort;

        public string Message => _model.Message;
        public string TrimmedMessage => _model.Message.Trim();
        public string MessageShort => _model.MessageShort;
        public string Parents => string.Join(", ", _model.ParentShaShorts);
        public string Hash => $"{_model.Sha} [{_model.ShaShort}]";
        public string Auther => $"{_model.AutherName} <{_model.AutherEmail}>";
        public string Date => _model.When.ToString("F");

        #region AutherImage

        private BitmapImage _autherImage;

        public BitmapImage AutherImage
        {
            get
            {
                if (_autherImage == null)
                    _autherImage = GravatarLoader.GetFromCache(_model.AutherEmail);

                if (_autherImage != null)
                    return _autherImage;

                lock (_isDownloadingSync)
                {
                    if (_isDownloading)
                        return null;

                    _isDownloading = true;
                }

                Task.Run(
                    () =>
                    {
                        var image = GravatarLoader.Get(_model.AutherEmail);

                        Livet.DispatcherHelper.UIDispatcher.Invoke(() => AutherImage = image);

                        _isDownloading = false;
                        _disposeResetEvent?.Set();
                    });

                return null;
            }
            set { SetProperty(ref _autherImage, value); }
        }

        private volatile bool _isDownloading;
        private readonly object _isDownloadingSync = new object();

        #endregion

        private ReadOnlyReactiveCollection<ChangeFileVm> _changeFiles;

        public ReadOnlyReactiveCollection<ChangeFileVm> ChangeFiles
        {
            get
            {
                lock (_changeFilesSync)
                {
                    if (_changeFiles != null)
                        return _changeFiles;

                    _changeFiles = _model.ChangeFiles
                        .ToReadOnlyReactiveCollection(x => new ChangeFileVm(x))
                        .AddTo(MultipleDisposable);
                }

                MultipleDisposable.Add(() => _changeFiles.ForEach(x => x.Dispose()));

                ChangeFiles.ObserveAddChanged().Subscribe(x =>
                {
                    if (SelectedChangeFiles.Any() == false)
                        SelectedChangeFiles.Add(x);
                }).AddTo(MultipleDisposable);

                return _changeFiles;
            }
        }

        private readonly object _changeFilesSync = new object();


        public ObservableCollection<ChangeFileVm> SelectedChangeFiles { get; }

        private object _diffFileViewSource;

        public object DiffFileViewSource
        {
            get { return _diffFileViewSource; }
            set { SetProperty(ref _diffFileViewSource, value); }
        }

        public ReadOnlyReactiveProperty<bool> IsChangeFilesBuilding { get; }

        private readonly RepositoryVm _repos;
        private readonly Model.Git.Commit _model;

        private ReactiveCommand<ResetMode> _resetCommand;

        public ReactiveCommand<ResetMode> ResetCommand
        {
            get
            {
                if (_resetCommand != null)
                    return _resetCommand;

                _resetCommand = new ReactiveCommand<ResetMode>().AddTo(MultipleDisposable);
                _resetCommand.Subscribe(mode => _repos.Reset(mode, _model.Sha))
                    .AddTo(MultipleDisposable);

                return _resetCommand;
            }
        }

        private ReactiveCommand _revertCommand;

        public ReactiveCommand RevertCommand
        {
            get
            {
                if (_revertCommand != null)
                    return _revertCommand;

                _revertCommand = new ReactiveCommand().AddTo(MultipleDisposable);
                _revertCommand.Subscribe(mode => _repos.Revert(_model.Sha))
                    .AddTo(MultipleDisposable);

                return _revertCommand;
            }
        }

        public ObservableCollection<CommitLabelVm> CommitLabels { get; }

        public TwoPaneLayoutVm TwoPaneLayout { get; }

        private ManualResetEventSlim _disposeResetEvent;

        public DoneCommitVm(RepositoryVm repos, Model.Git.Commit model, TwoPaneLayoutVm twoPaneLayout)
        {
            Debug.Assert(model != null);
            _repos = repos;
            _model = model;

            TwoPaneLayout = twoPaneLayout ?? new TwoPaneLayoutVm().AddTo(MultipleDisposable);

            MultipleDisposable.AddFirst(() =>
            {
                lock (_isDownloadingSync)
                {
                    if (_isDownloading)
                        _disposeResetEvent = new ManualResetEventSlim();
                }
            });

            IsChangeFilesBuilding =
                model.ObserveProperty(x => x.IsChangeFilesBuilding)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(MultipleDisposable);

            SelectedChangeFiles = new ObservableCollection<ChangeFileVm>();
            SelectedChangeFiles.CollectionChangedAsObservable()
                .Subscribe(_ =>
                {
                    var count = SelectedChangeFiles.Count;

                    if (count == 0)
                        DiffFileViewSource = ChangeFiles.FirstOrDefault();
                    else if (count == 1)
                        DiffFileViewSource = SelectedChangeFiles.FirstOrDefault();
                    else
                        DiffFileViewSource = SelectedChangeFiles;
                })
                .AddTo(MultipleDisposable);

            CommitLabels = repos
                .GetCommitLabels(model.Sha)
                .Select(x => new CommitLabelVm(x))
                .ToObservableCollection();

            MultipleDisposable.Add(() => CommitLabels.ForEach(x => x.Dispose()));
        }
    }
}