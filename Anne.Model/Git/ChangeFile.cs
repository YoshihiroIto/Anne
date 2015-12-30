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
            
        public LibGit2Sharp.ChangeKind Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); } 
        }
            
        public bool IsBinary
        {
            get { return _isBinary; }
            set { SetProperty(ref _isBinary, value); } 
        }

        // ReSharper disable InconsistentNaming
        protected string _path;
        protected string _patch;
        protected int _linesAdded;
        protected int _linesDeleted;
        protected LibGit2Sharp.ChangeKind _status;
        protected bool _isBinary;
        // ReSharper restore InconsistentNaming
    }
}
