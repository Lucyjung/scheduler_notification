using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ScheduleNoti.Utilities;
namespace ScheduleNoti.RPA
{
    class Server
    {
        public static LINEData[] getStatus()
        {
            var msgList = new List<LINEData>();

            // Check Database
            var dt = SQL.sendSqlQuery("SELECT 1");

            if (dt.Rows.Count == 0)
            {
                LINEData expData = new LINEData();
                expData.message = "Unable to Connect Database";
                expData.stickerid = 123;
                expData.stickerPkg = 1;
                msgList.Add(expData);
            }

            // Check App Server
            Ping pingSender = new Ping();
            try
            {
                PingReply reply = pingSender.Send(Config.appServer);
                if (reply.Status != IPStatus.Success)
                {
                    LINEData expData = new LINEData();
                    expData.message = "Unable to Connect Application Server : " + Config.appServer;
                    expData.stickerid = 123;
                    expData.stickerPkg = 1;
                    msgList.Add(expData);
                }
            }
            catch (Exception)
            {
                LINEData expData = new LINEData();
                expData.message = "Unable to Connect Application Server : " + Config.appServer;
                expData.stickerid = 123;
                expData.stickerPkg = 1;
                msgList.Add(expData);
            }
            return msgList.ToArray();
        }
    }
}
