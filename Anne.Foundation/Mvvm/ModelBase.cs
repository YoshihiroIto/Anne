using System;
using StatefulModel;

namespace Anne.Foundation.Mvvm
{
    public class ModelBase : IDisposable
    {
        public MultipleDisposable MultipleDisposable { get; set; } = new MultipleDisposable();

        public ModelBase()
        {
            DisposableChecker.Add(this);
        }

        public void Dispose()
        {
            MultipleDisposable.Dispose();

            DisposableChecker.Remove(this);
        }
    }
}