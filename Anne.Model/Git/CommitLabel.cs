using Anne.Foundation;
using Anne.Foundation.Mvvm;

namespace Anne.Model.Git
{
    public class CommitLabel : ModelBase
    {
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public CommitLabelType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        private string _name;
        private CommitLabelType _type;
    }
}