using System;

namespace Anne.Foundation
{
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public string Summry { get; set; }
    }
}