using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using StatefulModel;

namespace Anne.Features
{
    public class CommitVm : ViewModelBase
    {
        public string Message => _model.Message;
        public string MessageShort => _model.MessageShort;

        public string Parents => string.Join(", ", _model.ParentShaShorts);
        public string Hash => $"{_model.Sha} [{_model.ShaShort}]";
        public string Auther => $"{_model.AutherName} <{_model.AutherEmail}>";
        public string Date => _model.When.ToString("F");

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

                {
                    _isDownloading = true;
                    Task.Run(
                        () =>
                        {
                            AutherImage = GravatarLoader.Get(_model.AutherEmail);
                            _isDownloading = false;
                        });
                }

                return null;
            }
            set { SetProperty(ref _autherImage, value); }
        }

        private volatile bool _isDownloading;

        private readonly Model.Git.Commit _model;

        public CommitVm(Model.Git.Commit model)
        {
            Debug.Assert(model != null);
            _model = model;
        }
    }
}