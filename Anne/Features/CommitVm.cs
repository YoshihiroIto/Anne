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
        public string Hash => string.Format($"{_model.Sha} [{_model.ShaShort}]");
        public string Auther => string.Format($"{_model.AutherName} <{_model.AutherEmail}>");
        public string Date => _model.When.ToString("F");

        private BitmapImage _autherImage;
        public BitmapImage AutherImage
        {
            get
            {
                if (_autherImage != null)
                    return _autherImage;

                if (_isDownloading)
                    return null;

                using (new AnonymousDisposable(() => _isDownloading = false))
                {
                    _isDownloading = true;
                    Task.Run(LoadAutherImage);
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

        private async Task LoadAutherImage()
        {
            BitmapImage bi;
            if (GravatarLoader.Cache.TryGetValue(_model.AutherEmail, out bi))
                AutherImage = bi;
            else
                AutherImage = await GravatarLoader.LoadImage(_model.AutherEmail);
        }
    }
}