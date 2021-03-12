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
        enum TaskName
        {
            MEC = 0,
            SERVER,
            AVAILABLE,
            FREEZEVM,
            BACKREC,
            HOUSEKEEPING,
            LAST
        }
        private static string[] APP_STATE = { "Start", "Running" };

        private static Scheduler[] tasks = new Scheduler[(int)TaskName.LAST];

        public Form1()
        {
            InitializeComponent();
            Config.GetConfigurationValue();
            
            SQL.InitSQL(Config.sqlConnectionString);
            for (int i = 0; i < (int)TaskName.LAST; i++)
            {
                tasks[i] = new Scheduler();
            }

            tasks[(int)TaskName.MEC].Init(Config.interval, RPA.MEC.getStatus, true);
            tasks[(int)TaskName.SERVER].Init(Config.serverInterval, RPA.Server.getStatus, false);
            tasks[(int)TaskName.AVAILABLE].Init(Config.availInterval, RPA.Server.getScheduleTerminatedStatus, true);
            tasks[(int)TaskName.FREEZEVM].Init(Config.checkFreezeInterval, RPA.Server.checkFreezeVM, false);
            tasks[(int)TaskName.BACKREC].Init(Config.bankRecInterval, RPA.BankRec.notifyOtherReport, true);
            tasks[(int)TaskName.HOUSEKEEPING].Init(Config.houseKeepingInterval, RPA.HouseKeeping.Execute, false);
            LogFile.WriteToFile("Start Program");
        }
        private void startTimers()
        {
            foreach (var task in tasks)
            {
                task.Enable();
            }
        }
        private void stopTimers()
        {
            foreach (var task in tasks){
                task.Disable();
            }
        }
        private void Stop_Click(object sender, EventArgs e)
        {
            if (Stop.Text == APP_STATE[0])
            {
                startTimers();
                Stop.Text = APP_STATE[1];
                LogFile.WriteToFile("Restart Program");
            }
            else
            {
                stopTimers();
                Stop.Text = APP_STATE[0];
                LogFile.WriteToFile("Stop Program");
            }
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
