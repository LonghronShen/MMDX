using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace SlimMMDXDemoFramework
{
    public class Clock
    {
        private bool isRunning;
        private readonly long frequency;
        private long count;
        private long remain;
        
        public decimal FPS { get; set; }
        public Clock()
        {
            frequency = Stopwatch.Frequency;
            FPS = 60m;
        }
        public void Start()
        {
            count = Stopwatch.GetTimestamp();
            isRunning = true;
        }
        public float Update()
        {
            float result = 0.0f;
            if (isRunning)
            {
                long last = count;
                count = Stopwatch.GetTimestamp();
                result = (float)(count - last) / frequency;
            }
            return result;
        }
        public void Sync()
        {
            long now = Stopwatch.GetTimestamp();
            long delta = now + remain - count;
            long fpscnt = (long)(frequency / FPS);
            if (delta < fpscnt)
                Thread.Sleep((int)((fpscnt - delta) * 1000 / frequency));
            else
                remain = (delta - fpscnt) % fpscnt;
        }
    }
}
