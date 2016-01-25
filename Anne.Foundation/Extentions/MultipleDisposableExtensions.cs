using System;
using StatefulModel;

namespace Anne.Foundation.Extentions
{
    public static class MultipleDisposableExtensions
    {
        public static void RemoveAndDispose(this MultipleDisposable self, IDisposable target)
        {
            if (target == null)
                return;

            self.Remove(target);
            target.Dispose();
        }
    }
}