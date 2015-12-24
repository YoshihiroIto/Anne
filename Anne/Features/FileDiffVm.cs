using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
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

            public int OldIndex { get; set; }
            public int NewIndex { get; set; }
            public LineTypes LineType { get; set; }
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

                                sb.AppendLine(l.Content.Substring(1));
                                
                                diffLinesTemp.Add(
                                    new DiffLine
                                    {
                                        OldIndex = oldIndex,
                                        NewIndex = newIndex,
                                        LineType = (DiffLine.LineTypes)l.Type
                                    });
                            }
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
    }
}