namespace Anne.Foundation.CommitGraph
{
    public class CommitGraphNodeCell
    {
        public bool IsNode { get; set; }
        public int InputIndex { get; set; }
        public int OutputIndex { get; set; }

        public CommitGraphNodeCell(bool isNode, int inputIndex, int outputIndex)
        {
            IsNode = isNode;
            InputIndex = inputIndex;
            OutputIndex = outputIndex;
        }

        public CommitGraphNodeCell(bool isNode, int index)
            : this(isNode, index, index)
        {
        }
    }
}