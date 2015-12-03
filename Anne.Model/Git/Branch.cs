using System.Diagnostics;
using System.Text.RegularExpressions;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Model.Git
{
    public class Branch : ModelBase
    {
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<bool> IsRemote { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsCurrent { get; } = new ReactiveProperty<bool>();

        private readonly LibGit2Sharp.Branch _internal;
        private readonly LibGit2Sharp.Repository _repos;

        public Branch(LibGit2Sharp.Branch src, LibGit2Sharp.Repository repos)
        {
            Debug.Assert(src != null);
            Debug.Assert(repos != null);

            _internal = src;
            _repos = repos;

            Name.AddTo(MultipleDisposable);
            IsRemote.AddTo(MultipleDisposable);
            IsCurrent.AddTo(MultipleDisposable);

            Name.Value = src.FriendlyName;
            IsRemote.Value = src.IsRemote;
            IsCurrent.Value = src.IsCurrentRepositoryHead;
        }

        private string LocalName
        {
            get
            {
                var m = Regex.Match(Name.Value, @"origin/(.*)");
                return m.Groups[1].Value;
            }
        }

        public void Checkout()
        {
            // ブランチを作る
            var newBranch = _repos.CreateBranch(LocalName, _internal.Tip);

            // 追跡する
            _repos.Branches.Update(newBranch, b => b.TrackedBranch = _internal.CanonicalName);

            // チェックアウト
//            _repos.Checkout(newBranch);
        }

        public void Remove()
        {
            _repos.Branches.Remove(_internal);
        }
    } 
}