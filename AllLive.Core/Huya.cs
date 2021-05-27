using AllLive.Core.Interface;
using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AllLive.Core.Danmaku;
using AllLive.Core.Helper;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace AllLive.Core
{
    public class Huya : ILiveSite
    {
        public string Name => "虎牙直播";
        public ILiveDanmaku GetDanmaku() => new HuyaDanmaku();
        public async Task<List<LiveCategory>> GetCategores()
        {
            List<LiveCategory> categories = new List<LiveCategory>() {
                new LiveCategory() {
                    ID="1",
                    Name="网游",
                },
                new LiveCategory() {
                    ID="2",
                    Name="单机",
                },
                new LiveCategory() {
                    ID="8",
                    Name="娱乐",
                },
                new LiveCategory() {
                    ID="3",
                    Name="手游",
                },
            };
            foreach (var item in categories)
            {
                item.Children = await GetSubCategories(item.ID);
            }
            return categories;
        }

        private async Task<List<LiveSubCategory>> GetSubCategories(string id)
        {
            List<LiveSubCategory> subs = new List<LiveSubCategory>();
            var result = await HttpUtil.GetString($"https://live.cdn.huya.com/liveconfig/game/bussLive?bussType={id}");
            var obj = JObject.Parse(result);
            foreach (var item in obj["data"])
            {
                subs.Add(new LiveSubCategory()
                {
                    Pic = $"https://huyaimg.msstatic.com/cdnimage/game/{ item["gid"].ToString()}-MS.jpg",
                    ID = item["gid"].ToString(),
                    ParentID = id,
                    Name = item["gameFullName"].ToString(),
                });
            }
            return subs;
        }
        public async Task<LiveCategoryResult> GetCategoryRooms(LiveSubCategory category, int page = 1)
        {
            LiveCategoryResult categoryResult = new LiveCategoryResult()
            {
                Rooms = new List<LiveRoomItem>(),

            };
            var result = await HttpUtil.GetString($"https://www.huya.com/cache.php?m=LiveList&do=getLiveListByPage&tagAll=0&gameId={category.ID}&page={page}");
            var obj = JObject.Parse(result);
            foreach (var item in obj["data"]["datas"])
            {
                categoryResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = item["screenshot"].ToString(),
                    Online = (int)item["totalCount"],
                    RoomID = item["profileRoom"].ToString(),
                    Title = item["roomName"].ToString(),
                    UserName = item["nick"].ToString(),
                });
            }
            categoryResult.HasMore = (int)obj["data"]["page"] < (int)obj["data"]["totalPage"];
            return categoryResult;
        }
        public async Task<LiveCategoryResult> GetRecommendRooms(int page = 1)
        {
            LiveCategoryResult categoryResult = new LiveCategoryResult()
            {
                Rooms = new List<LiveRoomItem>(),

            };
            var result = await HttpUtil.GetString($"https://www.huya.com/cache.php?m=LiveList&do=getLiveListByPage&tagAll=0&page={page}");
            var obj = JObject.Parse(result);

            foreach (var item in obj["data"]["datas"])
            {
                categoryResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = item["screenshot"].ToString(),
                    Online = (int)item["totalCount"],
                    RoomID = item["profileRoom"].ToString(),
                    Title = item["roomName"].ToString(),
                    UserName = item["nick"].ToString(),
                });
            }
            categoryResult.HasMore = (int)obj["data"]["page"] < (int)obj["data"]["totalPage"];
            return categoryResult;
        }
        public async Task<LiveRoomDetail> GetRoomDetail(object roomId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("user-agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1 Edg/91.0.4472.69");
            var result = await HttpUtil.GetString($"https://m.huya.com/{roomId}",headers);
            return new LiveRoomDetail()
            {
                Cover = result.MatchText(@"picURL.=.'(.*?)'",""),
                Online = int.Parse(result.MatchText(@"totalCount:.'(\d+)'")),
                RoomID = result.MatchText(@"<h2 class=""roomid"">.*?(\d+)</h2>"),
                Title = result.MatchText(@"liveRoomName.=.'(.*?)'", ""),
                UserName = result.MatchText(@"ANTHOR_NICK.=.'(.*?)'", ""),
                Introduction = "",
                Notice = result.MatchTextSingleline(@"<div class=""notice_content"">(.*?)</div>", "").Trim(' '),
                Status = bool.Parse(result.MatchText( @"ISLIVE.=.(\w+);", "true")),
                Data = Encoding.UTF8.GetString(Convert.FromBase64String(result.MatchText( @"liveLineUrl.=.""(.*?)"";"))),
                DanmakuData = new HuyaDanmakuArgs(long.Parse(result.MatchText( @"ayyuid:.'(\d+)',")), long.Parse(result.MatchText( @"TOPSID.=.'(\d+)';")), long.Parse(result.MatchText( @"SUBSID.=.'(\d+)';")))
            };
        }
        public async Task<LiveSearchResult> Search(string keyword, int page = 1)
        {
            LiveSearchResult searchResult = new LiveSearchResult()
            {
                Rooms = new List<LiveRoomItem>(),

            };
            var result = await HttpUtil.GetString($"https://search.cdn.huya.com/?m=Search&do=getSearchContent&q={ Uri.EscapeDataString(keyword)}&uid=0&v=4&typ=-5&livestate=0&rows=20&start={(page - 1) * 20}");
            var obj = JObject.Parse(result);

            foreach (var item in obj["response"]["3"]["docs"])
            {
                searchResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = item["game_screenshot"].ToString(),
                    Online = (int)item["game_total_count"],
                    RoomID = item["room_id"].ToString(),
                    Title = item["game_roomName"].ToString(),
                    UserName = item["game_nick"].ToString(),
                });
            }
            searchResult.HasMore = (int)obj["response"]["3"]["numFound"] > (page * 20);
            return searchResult;
        }
        public async Task<List<LivePlayQuality>> GetPlayQuality(LiveRoomDetail roomDetail)
        {
            List<LivePlayQuality> qualities = new List<LivePlayQuality>();
            var url = roomDetail.Data.ToString();
            //四条线路
            var tx_url = Regex.Replace(url, @".*?\.hls\.huya\.com", "https://tx.hls.huya.com");
            var bd_url = Regex.Replace(url, @".*?\.hls\.huya\.com", "https://bd.hls.huya.com");
            var ali_url = Regex.Replace(url, @".*?\.hls\.huya\.com", "https://al.hls.huya.com");
            var migu_url = Regex.Replace(url, @".*?\.hls\.huya\.com", "https://migu-bd.hls.huya.com");
            qualities.Add(new LivePlayQuality()
            {
                Quality = "720P",
                Data = new List<string>() {
                   tx_url,
                   bd_url,
                   ali_url,
                   migu_url,
                }
            });
            qualities.Add(new LivePlayQuality()
            {
                Quality = "1080P",
                Data = new List<string>() {
                    tx_url.Replace("_2000", "").Replace("ratio=2000&", ""),
                    bd_url.Replace("_2000", "").Replace("ratio=2000&", ""),
                    ali_url.Replace("_2000", "").Replace("ratio=2000&", ""),
                    migu_url.Replace("_2000", "").Replace("ratio=2000&", ""),
                }
            });
            return qualities;
        }
        public async Task<List<string>> GetPlayUrls(LiveRoomDetail roomDetail, LivePlayQuality qn)
        {
            return qn.Data as List<string>;
        }


      
    }
}
