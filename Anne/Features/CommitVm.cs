using System.Diagnostics;
using Anne.Foundation.Mvvm;

namespace Anne.Features
{
    public class CommitVm : ViewModelBase
    {
        public string Message => _model.Message;
        public string MessageShort => _model.MessageShort;

        public string Parents => string.Join(", ", _model.ParentShaShorts);
        public string Hash => string.Format($"{_model.Sha} [{_model.ShaShort}]");
        public string Auther => string.Format($"{_model.AutherName} <{_model.AutherEmail}>");
        public string Date => _model.When.ToString("F");

        private readonly Model.Git.Commit _model;

        public CommitVm(Model.Git.Commit model)
        {
            Debug.Assert(model != null);
            _model = model;
        }
    }
}