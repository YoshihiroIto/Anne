using System.Threading;
using Livet.Messaging;

namespace Anne.Foundation.Mvvm
{
    public class ViewModelBase : ModelBase
    {
        private InteractionMessenger _messenger;

        public InteractionMessenger Messenger
        {
            get { return LazyInitializer.EnsureInitialized(ref _messenger, () => new InteractionMessenger()); }
        }

        public ViewModelBase(bool disableDisposableChecker = false)
            : base(disableDisposableChecker)
        {
        }
    }
}