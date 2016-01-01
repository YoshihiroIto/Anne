using System.Diagnostics;
using System.Text.RegularExpressions;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;

namespace Anne.Model.Git
{
    public class Branch : ModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (!SetProperty(ref _name, value))
                    return;

                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(LocalName));

                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(RemoteName));
            }
        }

        private bool _isRemote;
        public bool IsRemote
        {
            get { return _isRemote; }
            set { SetProperty(ref _isRemote, value); } 
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { SetProperty(ref _isCurrent, value); } 
        }

        private static Regex BranchRegex { get; } = new Regex(@"(?<Remote>[0-9a-zA-Z_\-\.]*)/(?<Local>.*)", RegexOptions.Compiled);

        public string LocalName
        {
            get
            {
                if (IsRemote == false)
                    return Name;

                var m = BranchRegex.Match(Name);
                return m.Groups["Local"].Value;
            }
        }

        public string RemoteName
        {
            get
            {
                if (IsRemote == false)
                    return string.Empty;

                var m = BranchRegex.Match(Name);
                return m.Groups["Remote"].Value;
            }
        }

        public string TipSha => Internal.Tip.Sha;

        private readonly LibGit2Sharp.Repository _repos;
        private readonly string _sourceCanonicalName;
        private LibGit2Sharp.Branch Internal => _repos.Branches[_sourceCanonicalName];
        
        public Branch(string sourceCanonicalName, LibGit2Sharp.Repository repos)
        {
            Debug.Assert(string.IsNullOrEmpty(sourceCanonicalName) == false);
            Debug.Assert(repos != null);

            _sourceCanonicalName = sourceCanonicalName;
            _repos = repos;

            UpdateProps();
        }

        public void UpdateProps()
        {
            Name = Internal.FriendlyName;
            IsRemote = Internal.IsRemote;
            IsCurrent = Internal.IsCurrentRepositoryHead;
        }

        public void Checkout()
        {
            // ブランチを作る
            var newBranch = _repos.CreateBranch(LocalName, Internal.Tip);

            // 追跡する
            _repos.Branches.Update(newBranch, b => b.TrackedBranch = Internal.CanonicalName);

            // チェックアウト
            _repos.Checkout(newBranch);
        }

        public void Remove()
        {
            _repos.Branches.Remove(Internal);
        }

        public void Switch()
        {
            _repos.Checkout(Internal);
        }
    }
}