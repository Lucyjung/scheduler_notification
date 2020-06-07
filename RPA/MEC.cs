using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleNoti.Utilities;
using System.IO;
using System.Data;

namespace ScheduleNoti.RPA
{
    class MEC
    {
        public static string[] getStatus(string robotLogPath, ref bool isSysException)
        {
            var msgList = new List<string>();
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Temp\\";
            string tmpfile = path + "mecDaily.xlsx";
            bool isCompleted = false;
            string status = "\n";
            status += "================\n";
            status += "|      MEC Status      |\n";
            status += "================\n";
            
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (Directory.Exists(robotLogPath))
            {
                DirectoryInfo info = new DirectoryInfo(robotLogPath);
                FileInfo[] files = info.GetFiles( "*.xlsx", SearchOption.TopDirectoryOnly).OrderByDescending(p => p.CreationTime).ToArray();
                if (files.Count() > 0)
                {
                    File.Copy(files[0].FullName, tmpfile, true);

                    DataTable dt = ExcelData.readData(tmpfile);

                    msgList.Add(status);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string processName = dr["Mail Trigger Process Name"].ToString();
                        if (processName != "")
                        {
                            string taskStatus = dr["Task Status"].ToString();
                            if (taskStatus == "Completed")
                            {
                                if (isCompleted == false)
                                {
                                    msgList[0] += "Completed Processes : ";
                                }
                                isCompleted = true;
                                msgList[0] += "\n" + processName;
                            }
                            else
                            {
                                string remark = dr["Remark"].ToString();
                                string exceptionMsg = processName + "\n";
                                exceptionMsg += "Status : " + taskStatus + "\n";
                                if (taskStatus == "In Progress")
                                {
                                    exceptionMsg += "Running on : " + dr["VM Assignment"];
                                } else if (taskStatus.Contains("Exception"))
                                {
                                    exceptionMsg += "Reason : " + remark;
                                    string parameter = dr["Parameter"].ToString();
                                    exceptionMsg += "\nRerun : Run " + processName.Replace("_" + parameter, " " + parameter);
                                    if (!remark.Contains("Business"))
                                    {
                                        isSysException = true;
                                    }
                                } else if (taskStatus.Contains("SCHEDULED"))
                                {
                                    exceptionMsg += "Expected Time : " + dr["Start Time"];
                                }
                                
                                msgList.Add(exceptionMsg);
                                
                            }
                        }
                    }
                    if (isCompleted == false)
                    {
                        msgList[0] += "\n No Completed Found";
                    }
                } else
                {
                    msgList.Add("Daily File Not Found");
                }
                
            }
            else
            {
                msgList.Add("Invalid Path");
            }
            return msgList.ToArray();
        }
    }
}
