using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class FileDiff : ModelBase
    {
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); } 
        }

        public string Patch
        {
            get { return _patch; }
            set { SetProperty(ref _patch, value); } 
        }

        private string _path;
        private string _patch;
    }
}
