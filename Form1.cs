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
        Scheduler mecScheduler = new Scheduler();
        Scheduler serverScheduler = new Scheduler();
        Scheduler bankRecScheduler = new Scheduler();
        Scheduler availScheduler = new Scheduler();
        Scheduler checkFreezeScheduler = new Scheduler();
        private static string[] APP_STATE = { "Start", "Running" };
        public Form1()
        {
            InitializeComponent();
            Config.GetConfigurationValue();
            
            SQL.InitSQL(Config.sqlConnectionString);
            
            mecScheduler.Init(Config.interval, timerCallback);
            serverScheduler.Init(Config.serverInterval, serverTimerCallback);
            availScheduler.Init(Config.availInterval, availTimerCallback);
            checkFreezeScheduler.Init(Config.checkFreezeInterval, checkFreezeTimerCallback);
            _ = availNotiAsync();
            _ = DoMECAsync();
            
            bankRecScheduler.Init(Config.bankRecInterval, bankRecTimerCallback);
            _ = bankRecNotiAsync();

            LogFile.WriteToFile("Start Program");
        }
        
        private void Stop_Click(object sender, EventArgs e)
        {
            if (Stop.Text == APP_STATE[0])
            {
                mecScheduler.Init(Config.interval, timerCallback);
                Stop.Text = APP_STATE[1];
                LogFile.WriteToFile("Restart Program");
            }
            else
            {
                mecScheduler.Disable();
                Stop.Text = APP_STATE[0];
                LogFile.WriteToFile("Stop Program");
            }
        }
        private void timerCallback(string arg)
        {
            _ = DoMECAsync();
            
            LogFile.WriteToFile("Send Notification");
        }
        private void serverTimerCallback(string arg)
        {
            _ = DoStatusCheckAsync();
        }
        private void availTimerCallback(string arg)
        {
            _ = availNotiAsync();
        }
        private void bankRecTimerCallback(string arg)
        {
            _ = bankRecNotiAsync();
        }
        private void checkFreezeTimerCallback(string arg)
        {
            _ = checkFreezeNotiAsync();
        }
        private async Task DoMECAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    string dailyPath = Path.Combine(Config.mecDailyPath, DateTime.Now.ToString("yyyyMM"), DateTime.Now.ToString("yyyyMMdd"), "Robot Logs");

                    var mecMsg = RPA.MEC.getStatus(dailyPath, Config.mecInputPath);

                    LINE.sendNoti(Config.lineToken, mecMsg);
                }catch (Exception ex)
                {
                    LogFile.WriteToFile("Exception : " + ex.ToString());
                }
            });
        }
        private async Task DoStatusCheckAsync()
        {
            await Task.Run(async () =>
            {
                try
                {

                    var serverMsg = RPA.Server.getStatus();
                    LINE.sendNoti(Config.lineToken, serverMsg);
                }
                catch (Exception ex)
                {
                    LogFile.WriteToFile("Exception : " + ex.ToString());
                }
            });
        }
        private async Task bankRecNotiAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    RPA.BankRec.notifyOtherReport();
                }
                catch (Exception ex)
                {
                    LogFile.WriteToFile("Exception : " + ex.ToString());
                }
            });
        }
        private async Task availNotiAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var terminatedMsg = RPA.Server.getScheduleTerminatedStatus();
                    LINE.sendNoti(Config.lineToken, terminatedMsg);
                }
                catch (Exception ex)
                {
                    LogFile.WriteToFile("Exception : " + ex.ToString());
                }
            });
        }
        private async Task checkFreezeNotiAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    var msg = RPA.Server.checkFreezeVM();
                    LINE.sendNoti(Config.lineToken, msg);
                }
                catch (Exception ex)
                {
                    LogFile.WriteToFile("Exception : " + ex.ToString());
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
