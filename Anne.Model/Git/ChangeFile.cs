using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class ChangeFile : ModelBase
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

        public int LinesAdded
        {
            get { return _linesAdded; }
            set { SetProperty(ref _linesAdded, value); } 
        }
            
        public int LinesDeleted
        {
            get { return _linesDeleted; }
            set { SetProperty(ref _linesDeleted, value); } 
        }
            
        public LibGit2Sharp.Mode Mode
        {
            get { return _mode; }
            set { SetProperty(ref _mode, value); } 
        }

        // ReSharper disable InconsistentNaming
        protected string _path;
        protected string _patch;
        protected int _linesAdded;
        protected int _linesDeleted;
        protected LibGit2Sharp.Mode _mode;
        // ReSharper restore InconsistentNaming
    }
}
