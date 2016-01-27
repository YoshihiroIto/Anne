using System.Diagnostics;
using System.Linq;
using Anne.Foundation;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;

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
            get
            {
                if (_isGeneratedPatch == false)
                    MakePatch();

                return _patch.Value;
            }
            set
            {
                _patch.Value = value;
                RaisePropertyChanged();
            }
        }

#if false
        public int LinesAdded
        {
            get
            {
                if (_isGeneratedPatch == false)
                    MakePatch();

                return _linesAdded;
            }
            set { SetProperty(ref _linesAdded, value); }
        }

        public int LinesDeleted
        {
            get
            {
                if (_isGeneratedPatch == false)
                    MakePatch();

                return _linesDeleted;
            }
            set { SetProperty(ref _linesDeleted, value); }
        }
#endif

        public ChangeKind Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public bool IsBinary
        {
            get
            {
                if (_isGeneratedPatch == false)
                    MakePatch();

                return _isBinary;
            }
            set { SetProperty(ref _isBinary, value); }
        }

        public void CopyFrom(ChangeFile source)
        {
            Path = source.Path;
            Patch = source.Patch;
            //LinesAdded = source.LinesAdded;
            //LinesDeleted = source.LinesDeleted;
            Status = source.Status;
            IsBinary = source.IsBinary;
        }

        public ChangeFile()
        {
            _isGeneratedPatch = true;
        }

        public ChangeFile(Repository repos, Tree oldTree, Tree newTree)
        {
            _repos = repos;
            _oldTree = oldTree;
            _newTree = newTree;
            _isGeneratedPatch = false;
        }

        // ReSharper disable InconsistentNaming
        protected string _path;
        protected SavingMemoryString _patch = new SavingMemoryString();
        //protected int _linesAdded;
        //protected int _linesDeleted;
        protected ChangeKind _status;
        protected bool _isBinary;
        // ReSharper restore InconsistentNaming

        private void MakePatch()
        {
            if (_repos == null)
                return;

            var diff = _repos.Internal.Diff.Compare<Patch>(_oldTree, _newTree, new[] { Path }).FirstOrDefault();
            Debug.Assert(diff != null);

            _isBinary = diff.IsBinaryComparison;
            //_linesAdded = diff.LinesAdded;
            //_linesDeleted = diff.LinesDeleted;
            _patch.Value = diff.Patch;

            _repos = null;
            _oldTree = null;
            _newTree = null;

            _isGeneratedPatch = true;

            // ReSharper disable ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(IsBinary));
            //RaisePropertyChanged(nameof(LinesAdded));
            //RaisePropertyChanged(nameof(LinesDeleted));
            RaisePropertyChanged(nameof(Patch));
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private bool _isGeneratedPatch;
        private Repository _repos;
        private Tree _oldTree;
        private Tree _newTree;
    }
}