using AllLive.Core.Interface;
using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AllLive.Core.Danmaku;
using AllLive.Core.Helper;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;


#if !WINDOWS_UWP
using QuickJS;
#endif
/*
 * 参考：
 * https://github.com/wbt5/real-url/blob/master/douyu.py
 */
namespace AllLive.Core
{
    public class Douyu : ILiveSite
    {
        public string Name => "斗鱼直播";
        public ILiveDanmaku GetDanmaku() => new DouyuDanmaku();
        public async Task<List<LiveCategory>> GetCategores()
        {
            List<LiveCategory> categories = new List<LiveCategory>();
            var result = await HttpUtil.GetString("https://m.douyu.com/api/cate/list");
            var obj = JObject.Parse(result);
            var cate1 = obj["data"]["cate1Info"] as JArray;
            var cate2 = obj["data"]["cate2Info"] as JArray;
            foreach (var item in cate1)
            {
                var cate1Id = item["cate1Id"].ToString();
                var cate1Name = item["cate1Name"].ToString();
                List<LiveSubCategory> subCategories = new List<LiveSubCategory>();
                cate2.Where(x => x["cate1Id"].ToString() == cate1Id).ToList().ForEach(element =>
                {
                    subCategories.Add(new LiveSubCategory()
                    {
                        Pic = element["icon"].ToString(),
                        ID = element["cate2Id"].ToString(),
                        ParentID = cate1Id,
                        Name = element["cate2Name"].ToString(),
                    });
                });
               
                categories.Add(
                  new LiveCategory()
                  {
                      ID = cate1Id,
                      Name = cate1Name,
                      // 只取前30个子分类
                      Children = subCategories.Take(30).ToList()
                  }
                );
            }
            categories.Sort((x, y) => x.ID.CompareTo(y.ID));
            return categories;
        }

      
        public async Task<LiveCategoryResult> GetCategoryRooms(LiveSubCategory category, int page = 1)
        {
            LiveCategoryResult categoryResult = new LiveCategoryResult()
            {
                Rooms = new List<LiveRoomItem>(),

            };
            var result = await HttpUtil.GetString($"https://www.douyu.com/gapi/rkc/directory/mixList/2_{ category.ID}/{page}");
            var obj = JObject.Parse(result);

            foreach (var item in obj["data"]["rl"])
            {
                if (item["type"].ToInt32() == 1)
                    categoryResult.Rooms.Add(new LiveRoomItem()
                    {
                        Cover = item["rs16"].ToString(),
                        Online = item["ol"].ToInt32(),
                        RoomID = item["rid"].ToString(),
                        Title = item["rn"].ToString(),
                        UserName = item["nn"].ToString(),
                    });
            }
            categoryResult.HasMore = page < obj["data"]["pgcnt"].ToInt32();
            return categoryResult;
        }
        public async Task<LiveCategoryResult> GetRecommendRooms(int page = 1)
        {
            LiveCategoryResult categoryResult = new LiveCategoryResult()
            {
                Rooms = new List<LiveRoomItem>(),

            };
            var result = await HttpUtil.GetString($"https://www.douyu.com/japi/weblist/apinc/allpage/6/{page}");
            var obj = JObject.Parse(result);
            foreach (var item in obj["data"]["rl"])
            {
                categoryResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = item["rs16"].ToString(),
                    Online = item["ol"].ToInt32(),
                    RoomID = item["rid"].ToString(),
                    Title = item["rn"].ToString(),
                    UserName = item["nn"].ToString(),
                });
            }
            categoryResult.HasMore = page < obj["data"]["pgcnt"].ToInt32();
            return categoryResult;
        }
        public async Task<LiveRoomDetail> GetRoomDetail(object roomId)
        {
            var roomInfo = await GetRoomInfo(roomId.ToString());
            var jsEncResult = await HttpUtil.GetString($"https://www.douyu.com/swf_api/homeH5Enc?rids={roomId}", new Dictionary<string, string>()
            {
                { "referer", $"https://m.douyu.com/{roomId}"},
                { "user-agent","Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1 Edg/114.0.0.0" },
            });
            var crptext = JObject.Parse(jsEncResult)["data"][$"room{roomId}"].ToString();
           

            return new LiveRoomDetail()
            {
                Cover = roomInfo["room_pic"].ToString(),
                Online = ParseHotNum(roomInfo["room_biz_all"]["hot"].ToString()),
                RoomID = roomInfo["room_id"].ToString(),
                Title = roomInfo["room_name"].ToString(),
                UserName = roomInfo["owner_name"].ToString(),
                UserAvatar = roomInfo["owner_avatar"].ToString(),
                Introduction =roomInfo["show_details"].ToString(),
                Notice = "",
                Status = roomInfo["show_status"].ToInt32() == 1 && roomInfo["videoLoop"].ToInt32() != 1,
                DanmakuData = roomInfo["room_id"].ToString(),
                Data = await GetPlayArgs(crptext, roomInfo["room_id"].ToString()),
                Url = "https://www.douyu.com/" + roomId,
                IsRecord= roomInfo["videoLoop"].ToInt32() == 1,
            };
        }


        private async Task<JToken> GetRoomInfo(string roomId)
        {
            var result = await HttpUtil.GetString($"https://www.douyu.com/betard/{roomId}", new Dictionary<string, string>()
            {
                { "referer", $"https://www.douyu.com/{roomId}"},
                { "user-agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1823.43" },
            });
            var obj = JObject.Parse(result);
            return obj["room"];
        }

        private async Task<string> GetPlayArgs(string html, string rid)
        {
            //取加密的js
            html = Regex.Match(html, @"(vdwdae325w_64we[\s\S]*function ub98484234[\s\S]*?)function").Groups[1].Value;
            html = Regex.Replace(html, @"eval.*?;}", "strc;}");
            var vaa= Environment.CurrentDirectory ;
            
#if WINDOWS_UWP
            var jsonObj = new
            {
                html = html,
                rid = rid
            };
            var result = await HttpUtil.PostJsonString("http://alive.nsapps.cn/api/AllLive/DouyuSign", Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj));
            var obj = JObject.Parse(result);
            if (obj["code"].ToInt32() == 0)
            {
                return obj["data"].ToString();
            }
            return "";
#else
            using (QuickJSRuntime runtime = new QuickJSRuntime())
            using (QuickJSContext context = runtime.CreateContext())
            {
                var did = "10000000000000000000000000001501";
                var time = Core.Helper.Utils.GetTimestamp();

                context.Eval(html, "", JSEvalFlags.Global);
                //调用ub98484234函数，返回格式化后的js
                var jsCode = context.Eval("ub98484234()", "", JSEvalFlags.Global).ToString();

                var v = Regex.Match(jsCode, @"v=(\d+)").Groups[1].Value;
                //对参数进行MD5，替换掉JS的CryptoJS\.MD5
                var rb = Core.Helper.Utils.ToMD5(rid + did + time + v);

                var jsCode2 = Regex.Replace(jsCode, @"return rt;}\);?", "return rt;}");
                //设置方法名为sign
                jsCode2 = Regex.Replace(jsCode2, @"\(function \(", "function sign(");
                //将JS中的MD5方法直接替换成加密完成的rb
                jsCode2 = Regex.Replace(jsCode2, @"CryptoJS\.MD5\(cb\)\.toString\(\)", $@"""{rb}""");
                context.Eval(jsCode2, "", JSEvalFlags.Global);
                //返回参数
                var args = context.Eval($"sign('{rid}','{did}','{time}')", "", JSEvalFlags.Global).ToString();
                return args;
            }
#endif
        }

        public async Task<LiveSearchResult> Search(string keyword, int page = 1)
        {
            LiveSearchResult searchResult = new LiveSearchResult()
            {
                Rooms = new List<LiveRoomItem>(),

            };
            var result = await HttpUtil.GetString($"https://www.douyu.com/japi/search/api/searchShow?kw={ Uri.EscapeDataString(keyword)}&page={ page}&pageSize=20");
            var obj = JObject.Parse(result);

            foreach (var item in obj["data"]["relateShow"])
            {
                searchResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = item["roomSrc"].ToString(),
                    Online = ParseHotNum(item["hot"].ToString()),
                    RoomID = item["rid"].ToString(),
                    Title = item["roomName"].ToString(),
                    UserName = item["nickName"].ToString(),
                });
            }
            searchResult.HasMore = ((JArray)obj["data"]["relateShow"]).Count > 0;
            return searchResult;
        }
        public async Task<List<LivePlayQuality>> GetPlayQuality(LiveRoomDetail roomDetail)
        {
            var data = roomDetail.Data.ToString();
            data += $"&cdn=&rate=0";
            List<LivePlayQuality> qualities = new List<LivePlayQuality>();
            var result = await HttpUtil.PostString($"https://www.douyu.com/lapi/live/getH5Play/{ roomDetail.RoomID}", data);
            var obj = JObject.Parse(result);
            var cdns = new List<string>();
            foreach (var item in obj["data"]["cdnsWithName"])
            {
                cdns.Add(item["cdn"].ToString());
            }
            // 如果cdn以scdn开头，将其放到最后
            for (int i = 0; i < cdns.Count; i++)
            {
                if (cdns[i].StartsWith("scdn"))
                {
                    cdns.Add(cdns[i]);
                    cdns.RemoveAt(i);
                    break;
                }
            }


            foreach (var item in obj["data"]["multirates"])
            {
                qualities.Add(new LivePlayQuality()
                {
                    Quality = item["name"].ToString(),
                    Data = new KeyValuePair<int, List<string>>(item["rate"].ToInt32(), cdns),
                });
            }
            return qualities;
        }
        public async Task<List<string>> GetPlayUrls(LiveRoomDetail roomDetail, LivePlayQuality qn)
        {
            var args = roomDetail.Data.ToString();
            var data = (KeyValuePair<int, List<string>>)qn.Data;
            List<string> urls = new List<string>();
            foreach (var item in data.Value)
            {
                var url = await GetUrl(roomDetail.RoomID, args, data.Key, item);
                if (url.Length != 0)
                {
                    urls.Add(url);
                }
            }
            return urls;
        }

        private async Task<string> GetUrl(string rid, string args, int rate, string cdn = "")
        {
            try
            {
                args += $"&cdn={cdn}&rate={rate}";
                var result = await HttpUtil.PostString($"https://www.douyu.com/lapi/live/getH5Play/{rid}", args);
                var obj = JObject.Parse(result);
                return obj["data"]["rtmp_url"].ToString() + "/" + System.Net.WebUtility.HtmlDecode(obj["data"]["rtmp_live"].ToString());
            }
            catch (Exception)
            {

                return "";
            }

        }
        public async Task<bool> GetLiveStatus(object roomId)
        {
            var roomInfo = await GetRoomInfo(roomId.ToString());
            return roomInfo["show_status"].ToInt32() == 1 && roomInfo["videoLoop"].ToInt32() != 1;
        }
        public Task<List<LiveSuperChatMessage>> GetSuperChatMessages(object roomId)
        {
            return Task.FromResult(new List<LiveSuperChatMessage>());
        }

        private int ParseHotNum(string hn)
        {
            try
            {
                var num = double.Parse(hn.Replace("万", ""));
                if (hn.Contains("万"))
                {
                    num = num * 10000;
                }
                return int.Parse(num.ToString());
            }
            catch (Exception)
            {
                return -999;
            }

        }
    }


}
