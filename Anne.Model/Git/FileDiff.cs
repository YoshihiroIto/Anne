using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class FileDiff : ModelBase
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); } 
        }

        private string _patch;
        public string Patch
        {
            get { return _patch; }
            set { SetProperty(ref _patch, value); } 
        }
    }
}
