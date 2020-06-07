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
        public static string mecInputPath;
        public static string mecDailyPath;
        public static string lineToken;
        public static void GetConfigurationValue()
        {
            try
            {
;
                interval = int.Parse(ConfigurationManager.AppSettings["Interval"]);
                mecInputPath = ConfigurationManager.AppSettings["MECInputPath"];
                mecDailyPath = ConfigurationManager.AppSettings["MECDailyPath"];
                lineToken = ConfigurationManager.AppSettings["lineToken"];

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
