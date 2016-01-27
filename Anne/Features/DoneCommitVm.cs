using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            AutherImage = image;
                            _isDownloading = false;
                            _disposeResetEvent?.Set();
                        });
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
                return LazyInitializer.EnsureInitialized(
                    ref _changeFiles,
                    () =>
                    {
                        var changeFiles = _model.ChangeFiles
                            .ToReadOnlyReactiveCollection(x => new ChangeFileVm(x))
                            .AddTo(MultipleDisposable);

                        changeFiles.ObserveAddChanged().Subscribe(x =>
                        {
                            if (SelectedChangeFiles.Any() == false)
                                SelectedChangeFiles.Add(x);
                        }).AddTo(MultipleDisposable);

                        MultipleDisposable.Add(() => _changeFiles.ForEach(x => x.Dispose()));

                        return changeFiles;
                    });
            }
        }

        private ObservableCollection<ChangeFileVm> _selectedChangeFiles;

        public ObservableCollection<ChangeFileVm> SelectedChangeFiles
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _selectedChangeFiles,
                    () =>
                    {
                        var selectedChangeFiles = new ObservableCollection<ChangeFileVm>();
                        selectedChangeFiles.CollectionChangedAsObservable()
                            .Subscribe(_ =>
                            {
                                var count = _selectedChangeFiles.Count;

                                if (count == 0)
                                    DiffFileViewSource = ChangeFiles.FirstOrDefault();
                                else if (count == 1)
                                    DiffFileViewSource = _selectedChangeFiles.FirstOrDefault();
                                else
                                    DiffFileViewSource = _selectedChangeFiles;
                            })
                            .AddTo(MultipleDisposable);

                        return selectedChangeFiles;
                    });
            }
        }

        private object _diffFileViewSource;

        public object DiffFileViewSource
        {
            get { return _diffFileViewSource; }
            set { SetProperty(ref _diffFileViewSource, value); }
        }

        private ReadOnlyReactiveProperty<bool> _isChangeFilesBuilding;

        public ReadOnlyReactiveProperty<bool> IsChangeFilesBuilding
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _isChangeFilesBuilding,
                    () =>
                        _model.ObserveProperty(x => x.IsChangeFilesBuilding)
                            .ToReadOnlyReactiveProperty()
                            .AddTo(MultipleDisposable));
            }
        }

        private ReactiveCommand<ResetMode> _resetCommand;

        public ReactiveCommand<ResetMode> ResetCommand
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _resetCommand,
                    () =>
                    {
                        var command = new ReactiveCommand<ResetMode>().AddTo(MultipleDisposable);
                        command.Subscribe(mode => _repos.Reset(mode, _model.Sha))
                            .AddTo(MultipleDisposable);
                        return command;
                    });
            }
        }

        private ReactiveCommand _revertCommand;

        public ReactiveCommand RevertCommand
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _revertCommand,
                    () =>
                    {
                        var command = new ReactiveCommand().AddTo(MultipleDisposable);
                        command.Subscribe(mode => _repos.Revert(_model.Sha))
                            .AddTo(MultipleDisposable);
                        return command;
                    });
            }
        }

        private ObservableCollection<CommitLabelVm> _commitLabels;

        public ObservableCollection<CommitLabelVm> CommitLabels
        {
            get
            {
                return LazyInitializer.EnsureInitialized(
                    ref _commitLabels,
                    () =>
                    {
                        var labels = _repos
                            .GetCommitLabels(_model.Sha)
                            .Select(x => new CommitLabelVm(x))
                            .ToObservableCollection();

                        MultipleDisposable.Add(() => CommitLabels.ForEach(x => x.Dispose()));
                        return labels;
                    });
            }
        }

        public TwoPaneLayoutVm TwoPaneLayout { get; }

        private readonly RepositoryVm _repos;
        private readonly Model.Git.Commit _model;
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
        }
    }
}