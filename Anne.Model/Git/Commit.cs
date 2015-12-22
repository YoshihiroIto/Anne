using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    // ※プロパティは変わることがないので変更通知は送らない
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

        private readonly LibGit2Sharp.Commit _internal;

        public Commit(LibGit2Sharp.Commit src)
        {
            Debug.Assert(src != null);

            _internal = src;
        }
    }
}