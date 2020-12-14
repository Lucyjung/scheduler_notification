using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScheduleNoti.Utilities
{
    public class Config
    {
        public static int interval;
        public static int serverInterval;
        public static string mecInputPath;
        public static string mecDailyPath;
        public static string lineToken;
        public static string sqlConnectionString;
        public static string calendarPTT;
        public static string mecConfig;
        public static string appServer;
        public static int bankRecInterval;
        public static string bankRecMasterRefFile;
        public static string bankRecNotiConfigFile;
        public static void GetConfigurationValue()
        {
            try
            {
;
                interval = int.Parse(ConfigurationManager.AppSettings["Interval"]);
                serverInterval = int.Parse(ConfigurationManager.AppSettings["serverInterval"]);
                mecInputPath = ConfigurationManager.AppSettings["MECInputPath"];
                mecDailyPath = ConfigurationManager.AppSettings["MECDailyPath"];
                lineToken = ConfigurationManager.AppSettings["lineToken"];
                sqlConnectionString = ConfigurationManager.AppSettings["sqlConnectionString"];
                calendarPTT = ConfigurationManager.AppSettings["calendarPTT"];
                mecConfig = ConfigurationManager.AppSettings["MECConfig"];
                appServer = ConfigurationManager.AppSettings["appServer"];
                bankRecInterval = int.Parse(ConfigurationManager.AppSettings["bankRecInterval"]);
                bankRecMasterRefFile = ConfigurationManager.AppSettings["bankRecMasterRefFile"];
                bankRecNotiConfigFile = ConfigurationManager.AppSettings["bankRecNotiConfigFile"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
