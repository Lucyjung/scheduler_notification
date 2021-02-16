using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ScheduleNoti.Utilities
{
    class Scheduler
    {
        private Timer aTimer;
        private Action<string> aTimerCB;
        public void Init(int interval, Action<string> callback)
        {
            if (interval > 0)
            {
                // Timer Init 
                aTimer = new System.Timers.Timer();

                // Hook up the Elapsed event for the timer. 
                aTimer.Elapsed += OnTimedEvent;
                aTimer.Interval = interval * 1000;

                // Have the timer fire repeated events (true is the default)
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                aTimerCB = callback;
            }
            
        }
        public void Disable()
        {
            aTimer.Enabled = false;
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            aTimerCB("");
        }
    }
}
