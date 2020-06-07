using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScheduleNoti.Utilities;
namespace ScheduleNoti
{
    public partial class Form1 : Form
    {
        Scheduler scheduler = new Scheduler();
        private static string[] APP_STATE = { "Start", "Running" };
        public Form1()
        {
            InitializeComponent();
            Config.GetConfigurationValue();

            scheduler.Init(Config.interval, timerCallback);
            LogFile.WriteToFile("Start Program");
            _ = DoMECAsync();
        }
        
        private void Stop_Click(object sender, EventArgs e)
        {
            if (Stop.Text == APP_STATE[0])
            {
                scheduler.Init(Config.interval, timerCallback);
                Stop.Text = APP_STATE[1];
                LogFile.WriteToFile("Restart Program");
            }
            else
            {
                scheduler.Disable();
                Stop.Text = APP_STATE[0];
                LogFile.WriteToFile("Stop Program");
            }
        }
        private void timerCallback(string arg)
        {
            _ = DoMECAsync();
            
            LogFile.WriteToFile("Send Notification");
        }
        private async Task DoMECAsync()
        {
            await Task.Run(async () =>
            {
                string dailyPath = Path.Combine(Config.mecDailyPath, DateTime.Now.ToString("yyyyMM"), DateTime.Now.ToString("yyyyMMdd"), "Robot Logs");
                bool isSysExp = false;
                var mecMsg = RPA.MEC.getStatus(dailyPath, ref isSysExp);
                if (isSysExp)
                {
                    LINE.sendNoti(Config.lineToken, mecMsg, 2, 25);
                } else
                {
                    LINE.sendNoti(Config.lineToken, mecMsg);
                }
                
            });
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
