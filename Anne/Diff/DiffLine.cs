using ParseDiff;

namespace Anne.Diff
{
    public class DiffLine
    {
        public enum LineTypes
        {
            Normal = LineChangeType.Normal,
            Add = LineChangeType.Add,
            Delete = LineChangeType.Delete,
            //
            ChunckTag = -1
        }

        public class ContentDiff
        {
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
        }

        public int OldIndex { get; set; }
        public int NewIndex { get; set; }
        public LineTypes LineType { get; set; }
        public int Index { get; set; }
        public string Content { get; set; }
        public ContentDiff[] ContentDiffs { get; set; } // ない場合は null
    }
}