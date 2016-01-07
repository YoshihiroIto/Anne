using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Anne.Features.Interfaces;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Features
{
    public class DoneCommitVm : ViewModelBase, ICommitVm
    {
        // ICommitVm
        public string Summry => MessageShort;

        public string Message => _model.Message;
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

                if (_isDownloading)
                    return null;

                _isDownloading = true;

                Task.Run(
                    () =>
                    {
                        var image = GravatarLoader.Get(_model.AutherEmail);
                        _isDownloading = false;

                        Livet.DispatcherHelper.UIDispatcher.Invoke(() => AutherImage = image);
                    });

                return null;
            }
            set { SetProperty(ref _autherImage, value); }
        }

        private volatile bool _isDownloading;

        #endregion

        private ReadOnlyReactiveCollection<ChangeFileVm> _changeFiles;

        public ReadOnlyReactiveCollection<ChangeFileVm> ChangeFiles
        {
            get
            {
                if (_changeFiles != null)
                    return _changeFiles;

                _changeFiles = _model.ChangeFiles
                    .ToReadOnlyReactiveCollection(x => new ChangeFileVm(x))
                    .AddTo(MultipleDisposable);

                ChangeFiles.ObserveAddChanged().Subscribe(x =>
                {
                    if (SelectedChangeFiles.Any() == false)
                        SelectedChangeFiles.Add(x);
                }).AddTo(MultipleDisposable);

                return _changeFiles;
            }
        }

        public ObservableCollection<ChangeFileVm> SelectedChangeFiles { get; }
        public ReactiveProperty<object> DiffFileViewSource { get; }

        private readonly Model.Git.Commit _model;

        public ReactiveCommand<ResetMode> ResetCommand { get; }
        public ReactiveCommand RevertCommand { get; }
        public ObservableCollection<CommitLabelVm> CommitLabels { get; }

        public DoneCommitVm(RepositoryVm repos, Model.Git.Commit model)
        {
            Debug.Assert(model != null);
            _model = model;

            DiffFileViewSource = new ReactiveProperty<object>().AddTo(MultipleDisposable);

            SelectedChangeFiles = new ObservableCollection<ChangeFileVm>();
            SelectedChangeFiles.CollectionChangedAsObservable()
                .Subscribe(_ =>
                {
                    var count = SelectedChangeFiles.Count;

                    if (count == 0)
                        DiffFileViewSource.Value = ChangeFiles.FirstOrDefault();
                    else if (count == 1)
                        DiffFileViewSource.Value = SelectedChangeFiles.FirstOrDefault();
                    else
                        DiffFileViewSource.Value = SelectedChangeFiles;
                })
                .AddTo(MultipleDisposable);

            ResetCommand = new ReactiveCommand<ResetMode>().AddTo(MultipleDisposable);
            ResetCommand.Subscribe(mode => repos.Reset(mode, model.Sha))
                .AddTo(MultipleDisposable);

            RevertCommand = new ReactiveCommand().AddTo(MultipleDisposable);
            RevertCommand.Subscribe(mode => repos.Revert(model.Sha))
                .AddTo(MultipleDisposable);

            CommitLabels = repos
                .GetCommitLabels(model.Sha)
                .Select(x => new CommitLabelVm(x))
                .ToObservableCollection();

            new AnonymousDisposable(() => CommitLabels.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);
        }
    }
}