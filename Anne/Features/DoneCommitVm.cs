using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Anne.Features.Interfaces;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class DoneCommitVm : ViewModelBase, ICommitVm
    {
        // ICommitVm
        public string Caption => MessageShort;

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
                        AutherImage = GravatarLoader.Get(_model.AutherEmail);
                        _isDownloading = false;
                    });

                return null;
            }
            set { SetProperty(ref _autherImage, value); }
        }

        private volatile bool _isDownloading;

        #endregion

        private ReadOnlyReactiveCollection<ChangeFileVm> _fileDiffs;

        public ReadOnlyReactiveCollection<ChangeFileVm> FileDiffs
        {
            get
            {
                return _fileDiffs ?? (
                    _fileDiffs = _model.ChangeFiles
                        .ToReadOnlyReactiveCollection(x => new ChangeFileVm(x))
                        .AddTo(MultipleDisposable));
            }
        }

        public ReactiveProperty<ChangeFileVm> SelectedFilePatch { get; } = new ReactiveProperty<ChangeFileVm>();

        private readonly Model.Git.Commit _model;

        public DoneCommitVm(Model.Git.Commit model)
        {
            Debug.Assert(model != null);
            _model = model;

            SelectedFilePatch.AddTo(MultipleDisposable);
        }
    }
}