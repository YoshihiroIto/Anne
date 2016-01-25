using System;
using System.Reactive.Linq;
using System.Windows;

namespace Anne.Foundation
{
    public static class FrameworkElementExtensions
    {
        public static IObservable<SizeChangedEventArgs> SizeChangedAsObservable(this FrameworkElement self)
        {
            return Observable.FromEvent<SizeChangedEventHandler, SizeChangedEventArgs>(
                h => (_, e) => h(e),
                h => self.SizeChanged += h,
                h => self.SizeChanged -= h);
        }
    }
}