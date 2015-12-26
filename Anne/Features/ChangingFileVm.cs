using Anne.Foundation.Mvvm;

namespace Anne.Features
{
    public class ChangingFileVm : ViewModelBase
    {
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        public bool IsInStaging
        {
            get { return _isInStaging; }
            set { SetProperty(ref _isInStaging, value); }
        }

        private bool _isInStaging;
        private string _path;

        public ChangingFileVm()
            : base(true)
        {
        }
    }
}