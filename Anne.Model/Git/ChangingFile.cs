using System.Diagnostics;
using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class ChangingFile : ModelBase
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

        public ChangingFile(Repository repos, string path, bool isInStaging)
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

            _repos.Add(Path);
        }

        public void Unstage()
        {
            if (IsInStaging == false)
                return;

            _repos.CancelAdd(Path);
        }
    }
}