using System.Collections.Generic;

namespace Anne.Foundation.CommitGraph
{
    public class CommitGraphNode
    {
        private CommitGraphNodeCell _nodeCell;
        private readonly List<CommitGraphNodeCell> _cells = new List<CommitGraphNodeCell>();

        public int NodeIndex => _nodeCell.InputIndex;

        public void Clear()
        {
            _nodeCell = null;
            _cells.Clear();
        }

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