using System.Diagnostics;

namespace Anne.Model.Git
{
    public class WipFile : ChangeFile
    {
        public bool IsInStaging
        {
            get { return _isInStaging; }
            internal set { SetProperty(ref _isInStaging, value); }
        }

        public void CopyFrom(WipFile source)
        {
            IsInStaging = source.IsInStaging;

            base.CopyFrom(source);
        }

        private bool _isInStaging;

        private readonly Repository _repos;

        public WipFile(Repository repos, bool isInStaging, string path)
        {
            Debug.Assert(repos != null);
            _repos = repos;

            _isInStaging = isInStaging;

            _path = path;
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