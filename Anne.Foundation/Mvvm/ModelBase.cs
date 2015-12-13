using System;
using System.Runtime.CompilerServices;
using Livet;
using StatefulModel;

namespace Anne.Foundation.Mvvm
{
    public class ModelBase : NotificationObject, IDisposable
    {
        public MultipleDisposable MultipleDisposable { get; set; } = new MultipleDisposable();

        public ModelBase()
        {
            DisposableChecker.Add(this);
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;
 
            storage = value;

            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(propertyName);

            return true;
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                MultipleDisposable?.Dispose();
                DisposableChecker.Remove(this);
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}