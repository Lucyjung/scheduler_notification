using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ScheduleNoti.Utilities;
using System.IO.Compression;

namespace ScheduleNoti.RPA
{
    class HouseKeeping
    {
        public static LINEData[] Execute()
        {
            var msgList = new List<LINEData>();
            DataTable configTbl = ExcelData.readData(Config.houseKeepingConfigFile);

            foreach (DataRow config in configTbl.Rows)
            {
                try
                {
                    // Prepare
                    string backupPath = Path.Combine(config["BackupDirectory"].ToString(),DateTime.Now.ToString("yyyyMMdd") + "_" + config["Table"].ToString());
                    string zipPath = Path.Combine(config["BackupDirectory"].ToString(), DateTime.Now.ToString("yyyyMMdd") + "_" + config["Table"].ToString() + ".zip");
                    string status = "Not Started";
                    DateTime now = DateTime.Now;
                    if (config["LastCleanDate"].ToString() == now.Day.ToString() && 
                        config["LastCleanMonth"].ToString() == now.Month.ToString() &&
                        now.Hour < Int32.Parse(config["CleanTime"].ToString()))
                    {
                        continue;
                    }

                    if (File.Exists(zipPath))
                    {
                        File.Move(zipPath, zipPath.Replace(".zip", "_" + DateTime.Now.Ticks +".zip"));
                    }
                    if (!Directory.Exists(backupPath))
                    {
                        Directory.CreateDirectory(backupPath);
                    }
                    status = "Started";
                    // 1. Get Date 
                    bool isError = false;
                    var dateDt = SQL.sendSqlQuery(@"declare @daystokeep int;
                        set @daystokeep = " + config["DayToKeep"].ToString() + @";
                        declare @threshold datetime;
                        set @threshold = DATEADD(DAY, DATEDIFF(DAY, 0, GETDATE()), -@daystokeep);
                        select CAST(" + config["DateField"].ToString() + @" AS DATE) as dateCol
                        from " + config["Table"].ToString() + @"
                        where " + config["DateField"].ToString() + @" < @threshold
                        GROUP BY CAST(" + config["DateField"].ToString() + @" AS DATE)"
                        , ref isError);
                    LogFile.WriteToFile("1. House Keeping Get Table=> " + config["Table"].ToString());
                    if (!isError && dateDt.Rows.Count > 0)
                    {
                        // 2. Back up 
                        foreach (DataRow date in dateDt.Rows)
                        {
                            string workingDate = DateTime.Parse(date["dateCol"].ToString()).ToString("yyyy-MM-dd");
                            DataTable dailyDt = SQL.sendSqlQuery(@"select * from " + config["Table"].ToString() + @"
                                where CAST(" + config["DateField"].ToString() + @" AS DATE) = '" + workingDate + @"'"
                                , ref isError);
                            LogFile.WriteToFile("2. House Keeping Back up => " + workingDate);
                            if (!isError)
                            {
                                string[] ignoreCol = config["ignoreField"].ToString().Split(',');
                                foreach (string ignoreField in ignoreCol)
                                {
                                    if (ignoreField != "")
                                    {
                                        dailyDt.Columns.Remove(ignoreField);
                                    }
                                }
                                string dailyFile = Path.Combine(backupPath, workingDate + "_" + config["Table"].ToString() + ".csv");
                                ExcelData.saveTableToCSV(dailyFile, dailyDt);
                                LogFile.WriteToFile("3. House Keeping Save File => " + dailyFile);
                                // 3. Remove
                                SQL.sendSqlQuery(@"delete from " + config["Table"].ToString() + @"
                                where CAST(" + config["DateField"].ToString() + @" AS DATE) = '" + workingDate + @"'"
                                , ref isError);
                                LogFile.WriteToFile("4. House Keeping Remove => " + workingDate);
                            }
                        }
                        status = "Cleanup Completed";
                        ZipFile.CreateFromDirectory(backupPath, zipPath);
                        Directory.Delete(backupPath, true);
                        LogFile.WriteToFile("5. House Keeping Clean up=> " + config["Table"].ToString());
                    } 
                    if (isError)
                    {
                        status = "Error With DB";
                    } else if (dateDt.Rows.Count > 0)
                    {
                        status = "Completed with Empty";
                    }
         
                    config["LastCleanDate"] = DateTime.Now.Day.ToString();
                    config["LastCleanMonth"] = DateTime.Now.Month.ToString();
                    config["Status"] = status;
                } catch (Exception ex)
                {
                    LogFile.WriteToFile("Exception : " + ex.ToString());
                }
            }
            ExcelData.saveTableToExcel(Config.houseKeepingConfigFile, "Config", configTbl);
            return msgList.ToArray();
        }
        
    }
}
