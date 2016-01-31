using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Anne.Model.Git.CommitGraph
{
    public class CommitGraphNode
    {
#if false
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
#endif

        private CommitGraphNodeCell _nodeCell;
        private readonly List<CommitGraphNodeCell> _cells = new List<CommitGraphNodeCell>();

        public void AddNodeCell(int depth)
        {
            _nodeCell = new CommitGraphNodeCell(true, depth);
        }

        public void AddLineCell(int inputIndex, int outputIndex)
        {
            _cells.Add(new CommitGraphNodeCell(false, inputIndex, outputIndex));
        }
    }
}