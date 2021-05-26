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
                StringContent stringContent = new StringContent(data);
                var result = await httpClient.PostAsync(url, stringContent);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
