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
        public string Message => _internal.Message;
        public string MessageShort => _internal.MessageShort;

        public string Sha => _internal.Sha;
        public string ShaShort => _internal.Sha.Substring(0, 7);
        public IEnumerable<string> ParentShas => _internal.Parents.Select(x => x.Sha);
        public IEnumerable<string> ParentShaShorts => _internal.Parents.Select(x => x.Sha.Substring(0, 7));

        public string AutherName => _internal.Author.Name;
        public string AutherEmail => _internal.Author.Email;
        public DateTimeOffset When => _internal.Author.When;

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
        private readonly LibGit2Sharp.Commit _internal;

        public Commit(Repository repos, LibGit2Sharp.Commit src)
        {
            Debug.Assert(repos != null);
            Debug.Assert(src != null);

            _repos = repos;
            _internal = src;

            MultipleDisposable.Add(() => _changeFiles?.ForEach(f => f.Dispose()));
        }

        private IEnumerable<ChangeFile> FileDiffs
        {
            get
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var parent in _internal.Parents)
                {
                    foreach (var diff in _repos.Internal.Diff.Compare<TreeChanges>(parent.Tree, _internal.Tree))
                    {
                        yield return new ChangeFile(_repos, parent.Tree, _internal.Tree)
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