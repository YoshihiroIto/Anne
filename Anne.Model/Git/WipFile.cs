using System.Diagnostics;
using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class WipFile : ModelBase
    {
        public string Path
        {
            get { return _path; }
            internal set { SetProperty(ref _path, value); }
        }

        public bool IsInStaging
        {
            get { return _isInStaging; }
            internal set { SetProperty(ref _isInStaging, value); }
        }

        public string Patch
        {
            get { return _patch; }
            set { SetProperty(ref _patch, value); } 
        }

        private bool _isInStaging;
        private string _path;
        private string _patch;

        private readonly Repository _repos;

        public WipFile(Repository repos, string path, bool isInStaging)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            _path = path;
            _isInStaging = isInStaging;
        }

        public void Stage()
        {
            if (IsInStaging)
                return;

            _repos.Stage(Path);
        }

        public void Unstage()
        {
            if (IsInStaging == false)
                return;

            _repos.Unstage(Path);
        }
    }
}