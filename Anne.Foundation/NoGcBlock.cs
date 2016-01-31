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
            GC.TryStartNoGCRegion(128*1024*1024);

            Interlocked.Increment(ref _depth);
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref _depth);

            if (_depth == 0)
                if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                    GC.EndNoGCRegion();
        }
    }
}