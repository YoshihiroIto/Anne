using System;
using System.Runtime;

namespace Anne.Foundation
{
    public class NoGcBlock : IDisposable
    {
        public NoGcBlock()
        {
            GC.TryStartNoGCRegion(128*1024*1024);
        }

        public void Dispose()
        {
            if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                GC.EndNoGCRegion();

            GC.SuppressFinalize(this);
        }
    }
}