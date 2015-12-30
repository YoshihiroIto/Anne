using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Model.Git
{
    // ※プロパティは変わることがないので変更通知は送らない
    public class Commit : ModelBase
    {
        public string Message => Internal.Message;
        public string MessageShort => Internal.MessageShort;

        public string Sha => Internal.Sha;
        public string ShaShort => Internal.Sha.Substring(0, 7);
        public IEnumerable<string> ParentShas => Internal.Parents.Select(x => x.Sha);
        public IEnumerable<string> ParentShaShorts => Internal.Parents.Select(x => x.Sha.Substring(0, 7));

        public string AutherName => Internal.Author.Name;
        public string AutherEmail => Internal.Author.Email;
        public DateTimeOffset When => Internal.Author.When;

        private ObservableCollection<ChangeFile> _changeFiles = new ObservableCollection<ChangeFile>();

        public ObservableCollection<ChangeFile> ChangeFiles
        {
            set { SetProperty(ref _changeFiles, value); }

            get
            {
                if (_isFileDiffsMakeDone == false)
                {
                    _isFileDiffsMakeDone = true;
                    Task.Run(() => MakeFileDiffs().ForEach(x => ChangeFiles.Add(x)));
                }

                return _changeFiles;
            }
        }

        private volatile bool _isFileDiffsMakeDone;

        internal LibGit2Sharp.Commit Internal { get; }

        private readonly Repository _repos;

        public Commit(Repository repos, LibGit2Sharp.Commit src)
        {
            Debug.Assert(repos != null);
            Debug.Assert(src != null);

            _repos = repos;
            Internal = src;

            new AnonymousDisposable(() => _changeFiles?.ForEach(f => f.Dispose()))
                .AddTo(MultipleDisposable);
        }

        private IEnumerable<ChangeFile> MakeFileDiffs()
        {
            IEnumerable<ChangeFile> fileDiffs = null;

            _repos.ExecuteJobSync(
                "MakeFileDiffs()",
                () =>
                {
                    fileDiffs = Internal.Parents
                        .SelectMany(p => _repos.Internal.Diff.Compare<Patch>(p.Tree, Internal.Tree))
                        .Select(c =>
                            new ChangeFile
                            {
                                Path = c.Path,
                                Patch = c.Patch,
                                LinesAdded = c.LinesAdded,
                                LinesDeleted = c.LinesDeleted,
                                Status = c.Status
                            }
                        );
                });

            Debug.Assert(fileDiffs != null);
            return fileDiffs;
        }
    }
}