using System.Diagnostics;
using System.Windows.Media;
using Anne.Foundation;
using Anne.Foundation.Mvvm;

namespace Anne.Features
{
    public class CommitLabelVm : ViewModelBase
    {
        public string Name { get; }
        public CommitLabelType Type { get; }
        public Brush Background { get; }

        public CommitLabelVm(Model.Git.CommitLabel model)
        {
            Debug.Assert(model != null);

            Name = model.Name;
            Type = model.Type;
            Background = Brushes.MistyRose;
        }
    }
}