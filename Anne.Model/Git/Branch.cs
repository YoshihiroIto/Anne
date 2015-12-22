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
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(nameof(LocalName));
                    RaisePropertyChanged(nameof(RemoteName));
                }
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

        private readonly LibGit2Sharp.Branch _internal;
        private readonly LibGit2Sharp.Repository _repos;

        public string LocalName
        {
            get
            {
                if (IsRemote == false)
                    return Name;

                var m = Regex.Match(Name, @"([0-9a-zA-Z]*)/(.*)");
                return m.Groups[2].Value;
            }
        }

        public string RemoteName
        {
            get
            {
                if (IsRemote == false)
                    return string.Empty;

                var m = Regex.Match(Name, @"([0-9a-zA-Z]*)/(.*)");
                return m.Groups[1].Value;
            }
        }

        public Branch(LibGit2Sharp.Branch src, LibGit2Sharp.Repository repos)
        {
            Debug.Assert(src != null);
            Debug.Assert(repos != null);

            _internal = src;
            _repos = repos;

            UpdateProps();
        }

        public void UpdateProps()
        {
            Name = _internal.FriendlyName;
            IsRemote = _internal.IsRemote;
            IsCurrent = _internal.IsCurrentRepositoryHead;
        }

        public void Checkout()
        {
            // ブランチを作る
            var newBranch = _repos.CreateBranch(LocalName, _internal.Tip);

            // 追跡する
            _repos.Branches.Update(newBranch, b => b.TrackedBranch = _internal.CanonicalName);

            // チェックアウト
            _repos.Checkout(newBranch);
        }

        public void Remove()
        {
            _repos.Branches.Remove(_internal);
        }

        public void Switch()
        {
            _repos.Checkout(_internal);
        }
    }
}