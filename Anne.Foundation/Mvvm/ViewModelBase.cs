using Livet;

namespace Anne.Foundation.Mvvm
{
    public class ViewModelBase : ViewModel
    {
        public ViewModelBase()
        {
            DisposableChecker.Add(this);
        }

        protected override void Dispose(bool disposing)
        {
            DisposableChecker.Remove(this);

            base.Dispose(disposing);
        }
    }
}