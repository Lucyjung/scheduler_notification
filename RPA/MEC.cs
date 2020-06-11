using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleNoti.Utilities;
using System.IO;
using System.Data;
using System.Drawing;

namespace ScheduleNoti.RPA
{
    class MEC
    {
        public static LINEData[] getStatus(string robotLogPath, string inputPath)
        {
            var msgList = new List<LINEData>();
            var date = DateTime.Now;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Temp");
            string dailyfile = Path.Combine(path, "mecDaily.xlsx");
            string inputFile = Path.Combine(path, "mecInput.xlsm");
            bool isCompleted = false;
            bool isSysException = false;
            LINEData data = new LINEData();
            string status = "\n";
            status += "================\n";
            status += "|      MEC Status      |\n";
            status += "================\n";

            int curWorkingDay = BusinessDay.getCurrentWorkingDay(Config.calendarPTT);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (Directory.Exists(robotLogPath) && Directory.Exists(inputPath))
            {
                DirectoryInfo info = new DirectoryInfo(robotLogPath);
                FileInfo[] files = info.GetFiles( "*.xlsx", SearchOption.TopDirectoryOnly).OrderByDescending(p => p.CreationTime).ToArray();
                info = new DirectoryInfo(inputPath);
                FileInfo[] inputFiles = info.GetFiles("*.xlsm", SearchOption.TopDirectoryOnly).Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToArray();
                if (files.Count() > 0 && inputFiles.Count() > 0 && File.Exists(files[0].FullName) && File.Exists(inputFiles[0].FullName))
                {
                    if (File.Exists(dailyfile))
                    {
                        File.SetAttributes(dailyfile, FileAttributes.Normal);
                    }
                        
                    File.Copy(files[0].FullName, dailyfile, true);
                    if (File.Exists(inputFile))
                    {
                        File.SetAttributes(inputFile, FileAttributes.Normal);
                    }
                    
                    File.Copy(inputFiles[0].FullName, inputFile, true);

                    DataTable dailyDt = ExcelData.readData(dailyfile);
                    DataTable inputDt = ExcelData.readData(inputFile);
                    string dateField = "Monthly Date";
                    string schedule = "Monthly";

                    if ((date.Month%3) == 1)
                    {
                        dateField = "Quarterly Date";
                        schedule = "Quarterly";
                    }
                    string expression = "((([" + dateField + "] = '" + date.Day + "') AND ([Calendar Type] = 'Calendar')) OR (([" + dateField + "] = '" + curWorkingDay + "') AND ([Calendar Type] = 'Workday'))) AND ([Schedule] = '" + schedule + "')";
                    inputDt = inputDt.Select(expression).CopyToDataTable();
                    /* DataTable diffDt = dailyDt.AsEnumerable().Where(
                        r => !inputDt.AsEnumerable().Select(x => x["Mail Trigger Process Name"]).ToList().Contains(r["Mail Trigger Process Name"])).CopyToDataTable();*/
                    foreach (DataRow dr in dailyDt.Rows)
                    {
                        if (!inputDt.AsEnumerable().Select(x => x["Mail Trigger Process Name"]).ToList().Contains(dr["Mail Trigger Process Name"]))
                        {
                            DataRow toInsert = inputDt.NewRow();
                            toInsert["Mail Trigger Process Name"] = dr["Mail Trigger Process Name"];
                            toInsert["Task Status"] = dr["Task Status"];
                            toInsert["Parameter"] = dr["Parameter"];
                            toInsert["Trigger"] = dr["Trigger"];
                            toInsert["Notification Time"] = dr["Notification Time"];
                            toInsert["Process Dependencies"] = dr["Process Dependencies"];
                            toInsert["Remark"] = dr["Remark"];
                            inputDt.Rows.InsertAt(toInsert, inputDt.Rows.Count - 1);
                        }
                    }

                    DataRow[] filterDr = inputDt.Select();
                    if (filterDr.Length > 0)
                    {
                        data.message = status;
                        data.stickerid = 0;
                        data.stickerPkg = 0;
                        msgList.Add(data);
                        foreach (DataRow dr in filterDr)
                        {
                            string processName = dr["Mail Trigger Process Name"].ToString();
                            isSysException = false;
                            if (processName != "")
                            {
                                string taskStatus = dr["Task Status"].ToString();
                                string parameter = dr["Parameter"].ToString();
                                if (taskStatus == "Completed")
                                {
                                    if (isCompleted == false)
                                    {
                                        int total = filterDr.Length;
                                        int completedCnt = inputDt.Select("[Task Status]='Completed'").Length;
                                        msgList[0].message += "Completed Processes (" + completedCnt + "/" + total + ") : ";
                                        if (total == completedCnt)
                                        {
                                            msgList[0].stickerid = 22;
                                            msgList[0].stickerPkg = 2;
                                        }
                                    }
                                    isCompleted = true;
                                    msgList[0].message += "\n" + processName;
                                }
                                else if (taskStatus == "")
                                {
                                    string msg = processName + "\n";
                                    string trigger = dr["Trigger"].ToString();
                                    if (trigger == "Mail")
                                    {
                                        msg += "Status : Waiting for Mail\n";
                                        msg += "Run Command : "+ getRerunCmd(processName, parameter)  + "\n";
                                        msg += "Time : " + convertNotification(dr["Notification Time"]);
                                    } else if (trigger == "Task")
                                    {
                                        msg += "Status : Waiting for Task(s) to complete\n";
                                        msg += "Dependencies : " + dr["Process Dependencies"];
                                    } else
                                    {
                                        msg += "Trigger : "+ trigger + "\n";
                                        msg += "Dependencies : " + dr["Process Dependencies"];
                                    }
                                    LINEData pendingData = new LINEData();
                                    pendingData.message = msg;
                                    pendingData.stickerid = 0;
                                    pendingData.stickerPkg = 0;
                                    msgList.Add(pendingData);
                                }
                                else
                                {
                                    string remark = dr["Remark"].ToString();
                                    string exceptionMsg = processName + "\n";
                                    LINEData expData = new LINEData();
                                    exceptionMsg += "Status : " + taskStatus + "\n";
                                    if (taskStatus == "In Progress")
                                    {
                                        exceptionMsg += "Running on : " + dr["VM Assignment"];
                                    }
                                    else if (taskStatus.Contains("Exception"))
                                    {
                                        exceptionMsg += "Reason : " + remark;
                                        

                                        exceptionMsg += "\nRerun : Run " + getRerunCmd(processName, parameter);
                                        
                                        if (!remark.Contains("Business"))
                                        {
                                            isSysException = true;
                                        }
                                    }
                                    else if (taskStatus.Contains("SCHEDULED"))
                                    {
                                        exceptionMsg += "Expected Time : " + convertNotification(dr["Start Time"]);
                                    } 
                                    if (isSysException)
                                    {
                                        expData.message = exceptionMsg;
                                        expData.stickerid = 173;
                                        expData.stickerPkg = 2;
                                        msgList.Add(expData);
                                    }
                                    else
                                    {
                                        expData.message = exceptionMsg;
                                        expData.stickerid = 0;
                                        expData.stickerPkg = 0;
                                        msgList.Add(expData);
                                    }
                                }
                            }
                        }
                    }
                    
                    if (isCompleted == false)
                    {
                        msgList[0].message += "\n No Completed Found";
                    }
                }
            }
            return msgList.ToArray();
        }
        private static string getRerunCmd(string processName, string parameter)
        {
            string command;
            if (parameter != "")
            {
                command = processName.Replace("_" + parameter, " " + parameter);
            }
            else
            {
                command = processName;
            }
            return command;
        }
        private static string convertNotification(Object obj)
        {
            return obj.ToString().Replace("12/31/1899 ", "");
        }
    }
}
