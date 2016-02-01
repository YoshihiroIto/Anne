using System;
using System.Runtime;
using System.Threading;

namespace Anne.Foundation
{
    public class NoGcBlock : IDisposable
    {
        private static int _depth;

        public NoGcBlock()
        {
            try
            {
                GC.TryStartNoGCRegion(32 * 1024 * 1024);
            }
            catch
            {
                // ignored
            }

            Interlocked.Increment(ref _depth);
        }

        public void Dispose()
        {
            if (Interlocked.Decrement(ref _depth) == 0)
                if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                    GC.EndNoGCRegion();
        }
    }
}