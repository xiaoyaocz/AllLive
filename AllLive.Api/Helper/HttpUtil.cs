using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace AllLive.Core.Helper
{
    public static class HttpUtil
    {
        public static async Task<string> GetString(string url,IDictionary<string,string> headers=null)
        {
            using (HttpClient httpClient=new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }
               var result=await httpClient.GetAsync(url);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
        }
        public static async Task<string> PostString(string url, string data,IDictionary<string, string> headers = null)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }
                List<KeyValuePair<string, string>> body = new List<KeyValuePair<string, string>>();
                foreach (var item in data.Split('&'))
                {
                    var splits = item.Split('=');
                    body.Add(new KeyValuePair<string, string>(splits[0], splits[1]));
                }
                FormUrlEncodedContent content = new FormUrlEncodedContent(body);
                var result = await httpClient.PostAsync(url, content);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
