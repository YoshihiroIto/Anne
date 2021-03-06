﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Anne.Diff;
using Anne.Foundation;
using DiffMatchPatch;
using ParseDiff;
using Reactive.Bindings;

namespace Anne.Features.Interfaces
{
    public interface IFileDiffVm
    {
        ReactiveProperty<string> Path { get; }
        ReactiveProperty<DiffLine[]> DiffLines { get; }

        //ReactiveProperty<int> LinesAdded { get; }
        //ReactiveProperty<int> LinesDeleted { get; }
        ReactiveProperty<LibGit2Sharp.ChangeKind> Status { get; }
        ReactiveProperty<bool> IsBinary { get; }

        SavingMemoryString Diff { get; set; }
    }

    public static class FileDiffExtensions
    {
        public static void MakeDiff(this IFileDiffVm self, string patch)
        {
            var sb = new StringBuilder();
            {
                var diffLinesTemp = new List<DiffLine>();

                try
                {
                    var fileDiffs = ParseDiff.Diff.Parse(patch).FirstOrDefault();
                    if (fileDiffs != null)
                    {
                        foreach (var chunk in fileDiffs.Chunks)
                        {
                            sb.AppendLine(chunk.Content.TrimEnd('\r', '\n'));
                            diffLinesTemp.Add(new DiffLine { LineType = DiffLine.LineTypes.ChunkTag });

                            var oldIndex = chunk.OldStart - 1;
                            var newIndex = chunk.NewStart - 1;

                            var addDeletePairs = new Dictionary<int, DiffLine[]>();

                            foreach (var l in chunk.Changes)
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

                                sb.AppendLine(diffLine.Content.TrimEnd('\r', '\n'));

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

                self.DiffLines.Value = diffLinesTemp.ToArray();
            }

            self.Diff = new SavingMemoryString(sb.ToString().TrimEnd('\r', '\n'));
        }

        private static void MakeContentDiffs(Dictionary<int, DiffLine[]> addDeletePairs)
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

                    if (deleteContentDiffs.Any())
                        deleteLine.ContentDiffs = deleteContentDiffs.ToArray();

                    if (addContentDiffs.Any())
                        addLine.ContentDiffs = addContentDiffs.ToArray();
                });
        }
    }
}