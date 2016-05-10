using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenArt.Core.Classes.Misc
{
    public class StopWatchExt : Stopwatch
    {
        public void ResetStart()
        {
            this.Reset(); this.Start();
        }

        public long StopTicks(bool timespanTicks = false)
        {
            this.Stop();

            long res;
            if (timespanTicks)
            {
                 res = (long)((TimeSpan.TicksPerSecond * this.ElapsedTicks) / (double)Stopwatch.Frequency);

            }
            else
            {
                res = this.ElapsedTicks;
            }

            return res;
        }
    }
}
