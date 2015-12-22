using System.Diagnostics;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class CommitVm : ViewModelBase
    {
        public string Message { get; }
        public string MessageShort { get; }

        public CommitVm(Model.Git.Commit model)
        {
            Debug.Assert(model != null);

            Message = model.Message;
            MessageShort = model.MessageShort;
        }
    }
}