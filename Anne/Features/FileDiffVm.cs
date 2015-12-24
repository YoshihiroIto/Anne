using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class FileDiffVm : ViewModelBase
    {
        public string Path => _model.Path;
        public string Diff { get; private set; }

        public ReactiveProperty<bool> IsExpanded { get; } = new ReactiveProperty<bool>(false);

        private readonly FilePatch _model;

        public class DiffLine
        {
            public enum LineTypes
            {
                Normal = ParseDiff.LineChangeType.Normal,
                Add = ParseDiff.LineChangeType.Add,
                Delete = ParseDiff.LineChangeType.Delete,
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

                            foreach (var l in chunck.Changes)
                            {
                                sb.AppendLine(l.Content.Substring(1));
                                
                                diffLinesTemp.Add(
                                    new DiffLine
                                    {
                                        OldIndex = l.OldIndex,
                                        NewIndex = l.NewIndex,
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
        }
    }
}