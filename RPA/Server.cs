using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ScheduleNoti.Utilities;
namespace ScheduleNoti.RPA
{
    class Server
    {
        enum BPStatus
        {
            Pending = 0,
            Running,
            Terminated,
            Stopped,
            Completed,
            Debugging,
            Archived,
            Stopping,
            Warning
        }
        private static DataTable savedStatusDt = new DataTable();
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
        public static LINEData[] getScheduleTerminatedStatus()
        {
            var msgList = new List<LINEData>();
            DateTime endUtc = DateTime.UtcNow;
            DateTime startUtc = endUtc.AddSeconds(Config.availInterval * -1);
            bool isError = false;
            // Check Database
            var dt = SQL.sendSqlQuery(@"SELECT 
	              BPASchedule.name as ScheduleName
	              ,BPATask.name as TaskName
                  ,[entrytime]
                  ,[terminationreason]
              FROM [PTTRPA].[dbo].[BPAScheduleLogEntry]
              JOIN [BPAScheduleLog] ON [BPAScheduleLog].[id] = BPAScheduleLogEntry.schedulelogid
              JOIN [BPASchedule] ON BPASchedule.id = BPAScheduleLog.scheduleid
              JOIN BPATask ON BPATask.[id] = BPAScheduleLogEntry.taskid
              Where  (entrytime BETWEEN '" + startUtc.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"' AND  '" + endUtc.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"') AND (terminationreason is not null)
              ORDER BY entrytime DESC"
                , ref isError);
            if (isError)
            {
                LINEData expData = new LINEData();
                expData.message = "Unable to Connect Database";
                expData.stickerid = 123;
                expData.stickerPkg = 1;
                msgList.Add(expData);
            }
            else if (dt.Rows.Count > 0)
            {
                string[] ignoreList = Config.ignoreAvailSchedule.Split(',');
                foreach (DataRow dr in dt.Rows)
                {
                    bool isIgnore = false;
                    foreach (string ignore in ignoreList)
                    {
                        if (dr["ScheduleName"].ToString().Contains(ignore))
                        {
                            isIgnore = true;
                        }
                    }
                    if (isIgnore == false)
                    {
                        LINEData expData = new LINEData();
                        expData.message = "Schedule : " + dr["ScheduleName"].ToString();
                        expData.message += "\nTask : " + dr["TaskName"].ToString();
                        expData.message += "\nReason : " + dr["terminationreason"].ToString();
                        expData.message += "\nTime : " + DateTime.Parse(dr["entrytime"].ToString()).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                        msgList.Add(expData);
                    }
                    
                }
                if (msgList.Count > 0)
                {
                    LINEData expTotalData = new LINEData();
                    expTotalData.message = "Total Terminated : " + msgList.Count;
                    expTotalData.stickerid = 173;
                    expTotalData.stickerPkg = 2;
                    msgList.Add(expTotalData);
                }
                
            }
            var resDt = SQL.sendSqlQuery(@"SELECT
                  [name]
                  ,[status]
                  ,[DisplayStatus]
              FROM [BPAResource]
              Where DisplayStatus = 'Missing' And name NOT Like '%-U%'");
            if (resDt.Rows.Count > 0)
            {
                foreach (DataRow dr in resDt.Rows)
                {
                    LINEData expData = new LINEData();
                    expData.message = "Resource : " + dr["name"].ToString();
                    expData.message += "\nDisplayStatus : " + dr["DisplayStatus"].ToString();
                    msgList.Add(expData);
                }
            }
            return msgList.ToArray();
        }
        public static LINEData[] checkFreezeVM()
        {
            var msgList = new List<LINEData>();
            string[] VMs = Config.checkFreezeVM.Split(',');
            bool isError = false;
            // Check Database
            var dt = SQL.sendSqlQuery(@"SELECT TOP (" + VMs.Length + @") 
                [statusid]
                ,[BPASession].[lastupdated]
                ,[laststage]
	            ,name
                FROM [dbo].[BPASession] 
                inner join [BPAResource] on starterresourceid  = [BPAResource].[resourceid]
                where name in ('" + Config.checkFreezeVM.Replace(",", "','") + @"')
                order by [BPASession].[lastupdated] DESC"
                , ref isError);
            if (!isError)
            {
                if (dt.Rows.Count > 0 && savedStatusDt.Rows.Count > 0)
                {
                    foreach (string vm in VMs)
                    {
                        bool success = false;
                        DataRow[] curStatus = dt.Select("[name]='" + vm + "'");
                        DataRow[] saved = savedStatusDt.Select("[name]='" + vm + "'");
                        int status;

                        success = Int32.TryParse(curStatus[0][0].ToString(), out status); // index 0 = statusid
                        if (success && (status == (int)BPStatus.Running || status == (int)BPStatus.Warning))
                        {
                            if (curStatus[0][2].ToString() == saved[0][2].ToString() && curStatus[0][1].ToString() == saved[0][1].ToString()) // index 2 = laststage, 1 = lastupdated
                            {
                                LINEData expTotalData = new LINEData();
                                expTotalData.message = "\nVM : " + vm + " has been showing same stage for " + Config.checkFreezeInterval + " Seconds" +
                                    "\nStage : " + curStatus[0][2].ToString() +  // index 2 = laststage
                                    "\nLast Update : " + curStatus[0][2].ToString(); // index 2 = laststage
                                expTotalData.stickerid = 173;
                                expTotalData.stickerPkg = 2;
                                msgList.Add(expTotalData);
                            }
                        }
                    }
                }
                savedStatusDt = dt.Copy();
            }
            
            return msgList.ToArray();
        }
    }
}
