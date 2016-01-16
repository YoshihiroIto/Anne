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

        internal LibGit2Sharp.Commit Internal { get; }

        private readonly Repository _repos;

        public Commit(Repository repos, LibGit2Sharp.Commit src)
        {
            Debug.Assert(repos != null);
            Debug.Assert(src != null);

            _repos = repos;
            Internal = src;

            MultipleDisposable.Add(() => _changeFiles?.ForEach(f => f.Dispose()));
        }

        private IEnumerable<ChangeFile> FileDiffs
        {
            get
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var parent in Internal.Parents)
                {
                    foreach (var diff in _repos.Internal.Diff.Compare<TreeChanges>(parent.Tree, Internal.Tree))
                    {
                        yield return new ChangeFile(_repos, parent.Tree, Internal.Tree)
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