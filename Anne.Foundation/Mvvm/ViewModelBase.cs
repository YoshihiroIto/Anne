using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Livet;

namespace Anne.Foundation.Mvvm
{
    public class ViewModelBase : ViewModel
    {
        private readonly bool _disableDisposableChecker;

        public ViewModelBase(bool disableDisposableChecker = false)
        {
            _disableDisposableChecker = disableDisposableChecker;

            if (_disableDisposableChecker == false)
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
            {
                if (_disableDisposableChecker == false)
                    DisposableChecker.Remove(this);
            }

            base.Dispose(disposing);
        }
    }
}