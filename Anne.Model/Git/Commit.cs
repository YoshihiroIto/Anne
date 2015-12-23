using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Anne.Foundation;
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

        private ObservableCollection<FilePatch> _filePatches;
        public ObservableCollection<FilePatch> FilePatches => _filePatches ?? (_filePatches = MakeFilePatches());

        internal LibGit2Sharp.Commit Internal { get; }

        private readonly Repository _repos;

        public Commit(Repository repos, LibGit2Sharp.Commit src)
        {
            Debug.Assert(repos != null);
            Debug.Assert(src != null);

            _repos = repos;
            Internal = src;

            new AnonymousDisposable(() =>
            {
                _filePatches?.ForEach(f => f.Dispose());
            }).AddTo(MultipleDisposable);
        }

        private ObservableCollection<FilePatch> MakeFilePatches()
        {
            return
                Internal.Parents
                    .SelectMany(p => _repos.Internal.Diff.Compare<Patch>(p.Tree, Internal.Tree))
                    .Select(c =>
                        new FilePatch
                        {
                            Path = c.Path,
                            Patch = c.Patch
                        }
                    ).ToObservableCollection();

#if false
            var changed = _repos.Internal.Diff.Compare<Patch>(Internal.Parents.First().Tree, Internal.Tree);

            return
                changed.Select(c =>
                    new FilePatch
                    {
                        Path = c.Path,
                        Patch = c.Patch
                    }).ToObservableCollection();
#endif
        }

#if false
        private string MakeDiff()
        {
            var sw = new Stopwatch();
            sw.Start();

            var changed = _repos.Internal.Diff.Compare<Patch>(Internal.Parents.First().Tree, Internal.Tree);

            var sb = new StringBuilder();

            foreach (var c in changed)
            {
                sb.AppendLine(c.Path + " ------------------------------------------->>>>");
                sb.AppendLine(c.Patch);
                sb.AppendLine("-------------------------------------------<<<<");
            }

            return
                "time:" + sw.ElapsedMilliseconds + "\n" +
                sb;
        }
#endif
    }
}