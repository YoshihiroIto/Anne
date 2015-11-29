using System;
using StatefulModel;

namespace Anne.Foundation
{
    public static class MultipleDisposableExtensions
    {
        public static void RemoveAndDispoes(this MultipleDisposable self, IDisposable target)
        {
            if (target == null)
                return;

            self.Remove(target);
            target.Dispose();
        }
    }
}