using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        private string LocalName
        {
            get
            {
                var m = Regex.Match(Name.Value, @"origin/(.*)");
                return m.Groups[1].Value;
            }
        }

        public Branch(LibGit2Sharp.Branch src, LibGit2Sharp.Repository repos)
        {
            Debug.Assert(src != null);
            Debug.Assert(repos != null);

            _internal = src;
            _repos = repos;

            Name.AddTo(MultipleDisposable);
            IsRemote.AddTo(MultipleDisposable);
            IsCurrent.AddTo(MultipleDisposable);

            UpdateProps();
        }

        public void UpdateProps()
        {
            Name.Value = _internal.FriendlyName;
            IsRemote.Value = _internal.IsRemote;
            IsCurrent.Value = _internal.IsCurrentRepositoryHead;
        }

        public async Task CheckoutAsync()
        {
            await Task.Run(() =>
            {
                // ブランチを作る
                var newBranch = _repos.CreateBranch(LocalName, _internal.Tip);

                // 追跡する
                _repos.Branches.Update(newBranch, b => b.TrackedBranch = _internal.CanonicalName);

                // チェックアウト
                _repos.Checkout(newBranch);
            });
        }

        public async Task RemoveAsync()
        {
            await Task.Run(() => _repos.Branches.Remove(_internal));
        }

        public async Task SwitchAsync()
        {
            await Task.Run(() => _repos.Checkout(_internal));
        }
    }
}