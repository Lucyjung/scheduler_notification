using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ScheduleNoti.Utilities
{
    public delegate LINEData[] aTimerCB();
    class Scheduler
    {
        private Timer aTimer;
        aTimerCB func;
        public void Init(int interval, aTimerCB callback, bool exec1stRun)
        {
            if (interval > 0)
            {
                // Timer Init 
                aTimer = new Timer();

                // Hook up the Elapsed event for the timer. 
                aTimer.Elapsed += OnTimedEvent;
                aTimer.Interval = interval * 1000;

                // Have the timer fire repeated events (true is the default)
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                func = callback;
                if (exec1stRun)
                {
                    _ = executeAsync();
                }
            }
        }
        public void Disable()
        {
            aTimer.Enabled = false;
        }
        public void Enable()
        {
            aTimer.Enabled = true;
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            _ = executeAsync();
        }
        public async Task executeAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var msg = func();
                    LINE.sendNoti(Config.lineToken, msg);
                }
                catch (Exception ex)
                {
                    LogFile.WriteToFile("Exception : " + ex.ToString());
                }
            });
        }
    }
}
