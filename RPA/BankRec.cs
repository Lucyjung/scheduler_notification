using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ScheduleNoti.Utilities;

namespace ScheduleNoti.RPA
{
    class BankRec
    {
        public static LINEData[] notifyOtherReport()
        {
            var msgList = new List<LINEData>();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            string masterRef = Path.Combine(path, "bankRecMaster.xlsx");
            File.Copy(Config.bankRecMasterRefFile, masterRef, true);
            DataTable master = ExcelData.readData(masterRef);
            DataTable configTbl = ExcelData.readData(Config.bankRecNotiConfigFile);
            foreach (DataRow dr in configTbl.Rows)
            {
                if (dr["BankAccount"].ToString() != "")
                {
                    var selected = master.Select("[Account_Number]='" + dr["BankAccount"]  + "'");
                    int curMonth = Int32.Parse(DateTime.Now.ToString("MM"));
                    int curDate = Int32.Parse(DateTime.Now.ToString("dd"));
                    DateTime now = DateTime.Now;
                    string[] timeStr = dr["Time"].ToString().Split(':');
                    DateTime settingTime = new DateTime(
                        now.Year,
                        now.Month,
                        now.Day,
                        int.Parse(timeStr[0]),
                        int.Parse(timeStr[1]),
                        0,
                        0, 
                        now.Kind
                        );
                    if (curMonth == Int32.Parse(dr["LastSentMonth"].ToString()))
                    {
                        continue;
                    } else if (now.TimeOfDay < settingTime.TimeOfDay)
                    {
                        continue;
                    }
                    else if (selected[0]["Cycle_" + dr["Cycle"]] != null)
                    {
                        int date = Int32.Parse(selected[0]["Cycle_" + dr["Cycle"]].ToString());
                        if (curDate == (date - 1))
                        {
                            Regex regex = new Regex(@"\{([^{}]+)\}*");
                            string body = dr["emailBody"].ToString();
                            
                            foreach (Match match in regex.Matches(body))
                            {
                                string str = match.Value;
                                string field = match.Value.Replace("{","").Replace("}","");
                                if (dr.Table.Columns.Contains(field))
                                {
                                    body = body.Replace(str, dr[field].ToString());
                                } else if (selected[0].Table.Columns.Contains(field))
                                {
                                    body = body.Replace(str, selected[0][field].ToString());
                                }
                            }
                            body = body.Replace("\n", "<br>");
                            dr["LastSentYear"] = DateTime.Now.ToString("yyyy");
                            dr["LastSentMonth"] = DateTime.Now.ToString("MM");
                            EmailOutlook.sendEmailViaOutlook(dr["Recipient"].ToString(), dr["Recipient_CC"].ToString(), dr["emailSubject"].ToString(), body, EmailOutlook.BodyType.HTML);
                        }
                    }
                }
            }
            ExcelData.saveTableToExcel(Config.bankRecNotiConfigFile, "Config", configTbl);
            
            return msgList.ToArray();
        }
    }
}
