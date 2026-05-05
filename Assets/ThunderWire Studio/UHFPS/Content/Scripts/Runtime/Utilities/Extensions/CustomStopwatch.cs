using System.Diagnostics;
using System;

namespace UHFPS.Tools
{
    public class CustomStopwatch : Stopwatch
    {
        public TimeSpan StartOffset { get; private set; }

        public CustomStopwatch() { }

        public CustomStopwatch(TimeSpan startOffset)
        {
            StartOffset = startOffset;
        }

        public new TimeSpan Elapsed
        {
            get { return base.Elapsed + StartOffset; }
            set { StartOffset = value; }
        }

        public new long ElapsedMilliseconds
        {
            get
            {
                return base.ElapsedMilliseconds + (long)StartOffset.TotalMilliseconds;
            }
        }

        public new long ElapsedTicks
        {
            get
            {
                return base.ElapsedTicks + StartOffset.Ticks;
            }
        }
    }
}