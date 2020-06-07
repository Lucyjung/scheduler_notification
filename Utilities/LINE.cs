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
        public static void sendNoti(string token, string[] msgs, int stickerPkg = 0, int stickerId = 0)
        {
            
            foreach (string msg in msgs)
            {
                List<KeyValuePair<string, string>> lineMsg = new List<KeyValuePair<string, string>>();
                lineMsg.Add(new KeyValuePair<string, string>("message", msg));
                if (stickerPkg != 0 && stickerId != 0)
                {
                    lineMsg.Add(new KeyValuePair<string, string>("stickerPackageId", stickerPkg.ToString()));
                    lineMsg.Add(new KeyValuePair<string, string>("stickerId", stickerId.ToString()));
                }
                HTTPRequest request = new HTTPRequest();
                _ = request.CurlRequestAsync(notifyUrl, "POST", lineMsg, "Bearer " + token);
                System.Threading.Thread.Sleep(1000);
            }
            
        }
    }
}
