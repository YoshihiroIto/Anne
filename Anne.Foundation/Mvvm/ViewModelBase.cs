using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Livet;

namespace Anne.Foundation.Mvvm
{
    public class ViewModelBase : ViewModel
    {
        public ViewModelBase()
        {
            DisposableChecker.Add(this);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
 
            storage = value;

            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposableChecker.Remove(this);

            base.Dispose(disposing);
        }
    }
}