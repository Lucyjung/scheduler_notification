using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleNoti.Utilities
{
    class LINE
    {
        private static string notifyUrl = "https://notify-api.line.me/api/notify";
        public static void sendNoti(string token, LINEData[] msgs)
        {
            
            foreach (var msg in msgs)
            {
                List<KeyValuePair<string, string>> lineMsg = new List<KeyValuePair<string, string>>();
                lineMsg.Add(new KeyValuePair<string, string>("message", msg.message));
                if (msg.stickerPkg != 0 && msg.stickerid != 0)
                {
                    lineMsg.Add(new KeyValuePair<string, string>("stickerPackageId", msg.stickerPkg.ToString()));
                    lineMsg.Add(new KeyValuePair<string, string>("stickerId", msg.stickerid.ToString()));
                }
                HTTPRequest request = new HTTPRequest();
                _ = request.CurlRequestAsync(notifyUrl, "POST", lineMsg, "Bearer " + token);
                System.Threading.Thread.Sleep(1000);
            }
            
        }
    }
    class LINEData
    {
        public string message { get; set; }
        public int stickerPkg { get; set; }
        public int stickerid { get; set; }
    }
}
