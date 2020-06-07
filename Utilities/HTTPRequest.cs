using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleNoti.Utilities
{
    class HTTPRequest
    {
        // Json request 
        public async System.Threading.Tasks.Task<HttpResponseMessage> CurlRequestAsync(string url, string method, string DATA = null, string auth = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                    {
                        if (auth != null)
                        {
                            request.Headers.TryAddWithoutValidation("Authorization", auth);
                        }
                        if (DATA != null)
                        {
                            request.Content = new StringContent(DATA, Encoding.UTF8, "application/json");
                        }
                        var result = await httpClient.SendAsync(request);
                        if (result.IsSuccessStatusCode)
                        {
                            using (HttpContent content = result.Content)
                            {

                                var jsonStr = content.ReadAsStringAsync().Result;

                                LogFile.WriteToFile("Server Respond : " + jsonStr);
                                callBackDelegate pFunc = new callBackDelegate(responseCallback);
                                pFunc(jsonStr);
                            }
                        }
                        else
                        {
                            LogFile.WriteToFile("Server Not Respond");
                        }
                        return result;

                    }
                }
            }
            catch (Exception e)
            {
                LogFile.WriteToFile("-----------------");
                LogFile.WriteToFile(e.Message);
                return null;
            }
        }
        public async System.Threading.Tasks.Task<HttpResponseMessage> CurlRequestAsync(string url, string method, List<KeyValuePair<string, string>> DATA = null, string auth = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                    {
                        if (auth != null)
                        {
                            request.Headers.TryAddWithoutValidation("Authorization", auth);
                        }
                        if (DATA != null)
                        {
                            request.Content = new FormUrlEncodedContent(DATA);
                        }
                        var result = await httpClient.SendAsync(request);
                        if (result.IsSuccessStatusCode)
                        {
                            using (HttpContent content = result.Content)
                            {

                                var jsonStr = content.ReadAsStringAsync().Result;

                                LogFile.WriteToFile("Server Respond : " + jsonStr);
                                callBackDelegate pFunc = new callBackDelegate(responseCallback);
                                pFunc(jsonStr);
                            }
                        }
                        else
                        {
                            LogFile.WriteToFile("Server Not Respond");
                        }
                        return result;

                    }
                }
            }
            catch (Exception e)
            {
                LogFile.WriteToFile("-----------------");
                LogFile.WriteToFile(e.Message);
                return null;
            }
        }
        private delegate void callBackDelegate(string res);
        private void responseCallback(string res)
        {
            try
            {

            }
            catch (Exception e)
            {
                LogFile.WriteToFile("Error during callback : " + e.ToString());
            }

        }
    }
}
