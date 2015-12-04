using System.Diagnostics;
using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class Commit : ModelBase
    {
        public string Message => _internal.Message;
        public string MessageShort => _internal.MessageShort;

        private readonly LibGit2Sharp.Commit _internal;

        public Commit(LibGit2Sharp.Commit src)
        {
            Debug.Assert(src != null);

            _internal = src;
        }
    }
}