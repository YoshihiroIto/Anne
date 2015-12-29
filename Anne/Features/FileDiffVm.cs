using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Anne.Features.Interfaces;
using Anne.Foundation.Mvvm;
using DiffMatchPatch;
using ParseDiff;
using FileDiff = Anne.Model.Git.FileDiff;

namespace Anne.Features
{
    public class FileDiffVm : ViewModelBase, IFileDiffVm
    {
        // IFileDiffVm
        public string Path => _model.Path;
        public string Diff { get; set; }
        public DiffLine[] DiffLines { get; set;  }

        private readonly FileDiff _model;

        public FileDiffVm(FileDiff model)
        {
            Debug.Assert(model != null);

            _model = model;

            this.MakeDiff(_model.Patch);
        }
    }
}