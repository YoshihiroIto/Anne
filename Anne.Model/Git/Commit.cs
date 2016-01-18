using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Anne.Foundation.Mvvm;
using LibGit2Sharp;

namespace Anne.Model.Git
{
    public class Commit : ModelBase
    {
        public string Message => Internal.Message;
        public string MessageShort => Internal.MessageShort;

        public string Sha { get; }
        public string ShaShort => Sha.Substring(0, 7);
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

                    IsChangeFilesBuilding = true;
                    Task.Run(() =>
                    {
                        FileDiffs.ForEach(x => ChangeFiles.Add(x));
                        IsChangeFilesBuilding = false;
                    });
                }

                return _changeFiles;
            }
        }

        private bool _isChangeFilesBuilding;

        public bool IsChangeFilesBuilding
        {
            get { return _isChangeFilesBuilding; }
            set { SetProperty(ref _isChangeFilesBuilding, value); }
        }

        private volatile bool _isFileDiffsMakeDone;
        private readonly Repository _repos;

        private LibGit2Sharp.Commit Internal => _repos.FindCommitBySha(Sha);

        public Commit(Repository repos, string commitSha)
        {
            Debug.Assert(repos != null);
            Debug.Assert(commitSha != null);

            _repos = repos;
            Sha = commitSha;

            MultipleDisposable.Add(() => _changeFiles?.ForEach(f => f.Dispose()));
        }

        private IEnumerable<ChangeFile> FileDiffs
        {
            get
            {
                // internalは都度作るのでここで一度作り使い回す
                var commit = Internal;

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var parent in commit.Parents)
                {
                    foreach (var diff in _repos.Internal.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree))
                    {
                        yield return new ChangeFile(_repos, parent.Tree, commit.Tree)
                        {
                            Path = diff.Path,
                            Status = diff.Status,
                        };
                    }
                }
            }
        }
    }
}