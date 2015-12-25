using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using DiffMatchPatch;
using ICSharpCode.AvalonEdit.Highlighting;
using ParseDiff;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class FileDiffVm : ViewModelBase
    {
        public string Path => _model.Path;
        public string Diff { get; private set; }
        public IHighlightingDefinition  SyntaxHighlighting { get; private set; }

        public ReactiveProperty<bool> IsExpanded { get; } = new ReactiveProperty<bool>(false);

        private readonly FilePatch _model;

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

        // ReSharper disable once NotAccessedField.Local
        public DiffLine[] DiffLines { get; private set;  }

        public FileDiffVm(FilePatch model)
        {
            Debug.Assert(model != null);

            _model = model;
            IsExpanded.AddTo(MultipleDisposable);

            MakeDiff();
            MakeSyntaxHighlighting();
        }

        private void MakeDiff()
        {
            var sb = new StringBuilder();
            {
                var diffLinesTemp = new List<DiffLine>();

                try
                {
                    var fileDiffs = ParseDiff.Diff.Parse(_model.Patch).FirstOrDefault();
                    if (fileDiffs != null)
                    {
                        foreach (var chunck in fileDiffs.Chunks)
                        {
                            sb.AppendLine(chunck.Content);
                            diffLinesTemp.Add(new DiffLine { LineType = DiffLine.LineTypes.ChunckTag });

                            var first = chunck.Changes.First();
                            var oldIndex = first.OldIndex - 1;
                            var newIndex = first.OldIndex - 1;

                            var addDeletePairs = new Dictionary<int, DiffLine[]>();

                            foreach (var l in chunck.Changes)
                            {
                                switch (l.Type)
                                {
                                    case LineChangeType.Normal:
                                        ++oldIndex;
                                        ++newIndex;
                                        break;

                                    case LineChangeType.Add:
                                        ++newIndex;
                                        break;

                                    case LineChangeType.Delete:
                                        ++oldIndex;
                                        break;
                                }

                                var diffLine = 
                                    new DiffLine
                                    {
                                        OldIndex = oldIndex,
                                        NewIndex = newIndex,
                                        LineType = (DiffLine.LineTypes)l.Type,
                                        Index = l.Index,
                                        Content = l.Content.Substring(1) 
                                    };

                                diffLinesTemp.Add(diffLine);

                                sb.AppendLine(diffLine.Content);

                                if (l.Type == LineChangeType.Delete)
                                {
                                    Debug.Assert(l.Index != 0);

                                    if (addDeletePairs.ContainsKey(l.Index) == false)
                                        addDeletePairs[l.Index] = new DiffLine[2];

                                    addDeletePairs[l.Index][0] = diffLine;
                                }
                                else if (l.Type == LineChangeType.Add)
                                {
                                    Debug.Assert(l.Index != 0);

                                    if (addDeletePairs.ContainsKey(l.Index) == false)
                                        addDeletePairs[l.Index] = new DiffLine[2];

                                    addDeletePairs[l.Index][1] = diffLine;
                                }
                            }

                            MakeContentDiffs(addDeletePairs);
                        }
                    }
                }
                catch
                {
                    diffLinesTemp.Clear();
                }

                DiffLines = diffLinesTemp.ToArray();
            }

            Diff = sb.ToString();

            if ((Diff.Length >= 2) && (Diff.Last() == '\n'))
                Diff = Diff.Substring(0, Diff.Length - 2);
        }

        private void MakeSyntaxHighlighting()
        {
            var ext = System.IO.Path.GetExtension(Path);
            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(ext);
        }

        private void MakeContentDiffs(Dictionary<int, DiffLine[]> addDeletePairs)
        {
            addDeletePairs.Values
                .Where(x => x.All(y => y != null))
                .ForEach(p =>
                {
                    var deleteLine = p[0];
                    var addLine = p[1];
                    var deleteContent = deleteLine.Content;
                    var addContent = addLine.Content;

                    var dmp = new diff_match_patch();
                    var diffs = dmp.diff_main(deleteContent, addContent);
                    dmp.diff_cleanupSemantic(diffs);

                    // var html = dmp.diff_prettyHtml(diffs);
                    // Debug.WriteLine(html);

                    var deleteIndex = 0;
                    var addIndex = 0;
                    var deleteContentDiffs = new List<DiffLine.ContentDiff>();
                    var addContentDiffs = new List<DiffLine.ContentDiff>();

                    diffs.ForEach(d =>
                    {
                        var diffTextLength = d.text.Length;

                        switch (d.operation)
                        {
                            case Operation.DELETE:
                                deleteContentDiffs.Add(new DiffLine.ContentDiff
                                {
                                    StartIndex = deleteIndex,
                                    EndIndex = deleteIndex + diffTextLength
                                });
                                deleteIndex += diffTextLength;
                                break;

                            case Operation.INSERT:
                                addContentDiffs.Add(new DiffLine.ContentDiff
                                {
                                    StartIndex = addIndex,
                                    EndIndex = addIndex + diffTextLength
                                });
                                addIndex += diffTextLength;
                                break;

                            case Operation.EQUAL:
                                deleteIndex += diffTextLength;
                                addIndex += diffTextLength;
                                break;
                        }
                    });

                    // １つ変更は全て変更ということなので対象としない
                    if (deleteContentDiffs.Any())
                        deleteLine.ContentDiffs = deleteContentDiffs.ToArray();

                    if (addContentDiffs.Any())
                        addLine.ContentDiffs = addContentDiffs.ToArray();
                });
        }
    }
}