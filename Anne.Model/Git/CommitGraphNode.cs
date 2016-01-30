using System.Diagnostics;

namespace Anne.Model.Git
{
    public class CommitGraphNode
    {
        private int _depth;

        public int Current { get; set; }

        public void Forward()
        {
            ++ _depth;
        }

        public void Back()
        {
            Debug.Assert(_depth >= 0);
            -- _depth;
        }

        public void PutCurrent()
        {
            Current = _depth;
        }
    }
}