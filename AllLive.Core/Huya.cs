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
using System.Linq;
using System.Net.WebSockets;
using System.Web;
using WebSocketSharp;
using System.Collections.Specialized;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using AllLive.Core.Models.Tars;

namespace AllLive.Core
{
    public class Huya : ILiveSite
    {
        public string Name => "虎牙直播";
        public ILiveDanmaku GetDanmaku() => new HuyaDanmaku();
        TupHttpHelper tupHttpHelper = new TupHttpHelper("http://wup.huya.com", "liveui");
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
                    Pic = $"https://huyaimg.msstatic.com/cdnimage/game/{item["gid"].ToString()}-MS.jpg",
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
                var cover = item["screenshot"].ToString();
                if (!cover.Contains("?"))
                {
                    cover += "?x-oss-process=style/w338_h190&";
                }
                var title = item["introduction"]?.ToString();
                if (string.IsNullOrEmpty(title))
                {
                    title = item["roomName"]?.ToString() ?? "";
                }
                categoryResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = cover,
                    Online = item["totalCount"].ToInt32(),
                    RoomID = item["profileRoom"].ToString(),
                    Title = title,
                    UserName = item["nick"].ToString(),
                });
            }
            categoryResult.HasMore = obj["data"]["page"].ToInt32() < obj["data"]["totalPage"].ToInt32();
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
                var cover = item["screenshot"].ToString();
                if (!cover.Contains("?"))
                {
                    cover += "?x-oss-process=style/w338_h190&";
                }
                var title = item["introduction"]?.ToString();
                if (string.IsNullOrEmpty(title))
                {
                    title = item["roomName"]?.ToString() ?? "";
                }
                categoryResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = cover,
                    Online = item["totalCount"].ToInt32(),
                    RoomID = item["profileRoom"].ToString(),
                    Title = title,
                    UserName = item["nick"].ToString(),
                });
            }
            categoryResult.HasMore = obj["data"]["page"].ToInt32() < obj["data"]["totalPage"].ToInt32();
            return categoryResult;
        }
        public async Task<LiveRoomDetail> GetRoomDetail(object roomId)
        {
            var jsonObj = await GetRoomInfo(roomId);
            var topSid = jsonObj["topSid"].ToInt64();
            var subSid = jsonObj["subSid"].ToInt64();

            var title = jsonObj["roomInfo"]["tLiveInfo"]["sIntroduction"].ToString();
            if (string.IsNullOrEmpty(title))
            {
                title = jsonObj["roomInfo"]["tLiveInfo"]["sRoomName"].ToString();
            }

            var uid = await GetUid();
            var uuid = GetUuid();
            var huyaLines = new List<HuyaLineModel>();
            var huyaBiterates = new List<HuyaBitRateModel>();
            //读取可用线路
            var lines = jsonObj["roomInfo"]["tLiveInfo"]["tLiveStreamInfo"]["vStreamInfo"]["value"];
            foreach (var item in lines)
            {
                if (!string.IsNullOrEmpty(item["sFlvUrl"]?.ToString()))
                {
                    huyaLines.Add(new HuyaLineModel()
                    {
                        Line = item["sFlvUrl"].ToString(),
                        LineType = HuyaLineType.FLV,
                        FlvAntiCode = item["sFlvAntiCode"].ToString(),
                        HlsAntiCode = item["sHlsAntiCode"].ToString(),
                        StreamName = item["sStreamName"].ToString(),
                    });
                }
                //HLS效果不好，暂不使用
                //if (!string.IsNullOrEmpty(item["sHlsUrl"]?.ToString()))
                //{
                //    huyaLines.Add(new HuyaLineModel()
                //    {
                //        Line = item["sHlsUrl"].ToString().Replace("http://", "").Replace("https://", ""),
                //        LineType = HuyaLineType.HLS,
                //    });
                //}
            }

            // 将AL的线路放到最后,AL的线路非常容易出现403
            huyaLines = huyaLines.OrderBy(x => x.Line.Contains("al.flv.")).ToList();

            //优先FLV
            //huyaLines=huyaLines.Where(x=>!x.Line.Contains("-game")).OrderBy(x=>x.LineType).ToList();

            //清晰度
            var biterates = jsonObj["roomInfo"]["tLiveInfo"]["tLiveStreamInfo"]["vBitRateInfo"]["value"];
            foreach (var item in biterates)
            {
                huyaBiterates.Add(new HuyaBitRateModel()
                {
                    BitRate = item["iBitRate"].ToInt32(),
                    Name = item["sDisplayName"].ToString(),
                });
            }
            var realRoomId = jsonObj["roomInfo"]["tLiveInfo"]["lProfileRoom"].ToInt32();
            if (realRoomId == 0)
            {
                realRoomId = jsonObj["roomInfo"]["tProfileInfo"]["lProfileRoom"].ToInt32();
            }

            return new LiveRoomDetail()
            {
                Cover = jsonObj["roomInfo"]["tLiveInfo"]["sScreenshot"].ToString(),
                Online = jsonObj["roomInfo"]["tLiveInfo"]["lTotalCount"].ToInt32(),
                RoomID = realRoomId.ToString(),
                Title = title,
                UserName = jsonObj["roomInfo"]["tProfileInfo"]["sNick"].ToString(),
                UserAvatar = jsonObj["roomInfo"]["tProfileInfo"]["sAvatar180"].ToString(),
                Introduction = jsonObj["roomInfo"]["tLiveInfo"]["sIntroduction"].ToString(),
                Notice = jsonObj["welcomeText"].ToString(),
                Status = jsonObj["roomInfo"]["eLiveStatus"].ToInt32() == 2,
                Data = new HuyaUrlDataModel()
                {
                    Url = "https:" + Encoding.UTF8.GetString(Convert.FromBase64String(jsonObj["roomProfile"]["liveLineUrl"].ToString())),
                    Lines = huyaLines,
                    BitRates = huyaBiterates,
                    Uid = uid,
                    UUid = uuid,
                },
                DanmakuData = new HuyaDanmakuArgs(
                    jsonObj["roomInfo"]["tLiveInfo"]["lYyid"].ToInt64(),
                    topSid,
                    subSid
                ),
                Url = "https://www.huya.com/" + roomId
            };
        }

        private async Task<JToken> GetRoomInfo(object roomId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("user-agent", "Mozilla/5.0 (Linux; Android 11; Pixel 5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.91 Mobile Safari/537.36 Edg/117.0.0.0");
            var result = await HttpUtil.GetString($"https://m.huya.com/{roomId}", headers);
            var jsonStr = Regex.Match(result, @"window\.HNF_GLOBAL_INIT.=.\{[\s\S]*?\}[\s\S]*?</script>", RegexOptions.Singleline).Groups[0].Value;
            jsonStr = Regex.Replace(jsonStr, @"window\.HNF_GLOBAL_INIT.=.", "").Replace("</script>", "");
            jsonStr = Regex.Replace(jsonStr, @"function.*?\(.*?\).\{[\s\S]*?\}", "\"\"");


            var jsonObj = JObject.Parse(jsonStr);

            var topSid = result.MatchText(@"lChannelId"":([0-9]+)").ToInt64();
            var subSid = result.MatchText(@"lSubChannelId"":([0-9]+)").ToInt64();

            jsonObj["topSid"] = topSid;
            jsonObj["subSid"] = subSid;

            return jsonObj;
        }
        private long GetUuid()
        {
            return (long)((DateTimeOffset.Now.ToUnixTimeMilliseconds() % 10000000000 * 1000 + (1000 * new Random().Next(0, int.MaxValue))) % uint.MaxValue);
        }
        private async Task<string> GetUid()
        {
            var data = "{\"appId\":5002,\"byPass\":3,\"context\":\"\",\"version\":\"2.4\",\"data\":{}}";
            var result = await HttpUtil.PostJsonString($"https://udblgn.huya.com/web/anonymousLogin", data);
            var obj = JObject.Parse(result);

            return obj["data"]["uid"].ToString();
        }

        public async Task<LiveSearchResult> Search(string keyword, int page = 1)
        {
            LiveSearchResult searchResult = new LiveSearchResult()
            {
                Rooms = new List<LiveRoomItem>(),

            };
            var headers = new Dictionary<string, string>()
            {
                { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0"},
                { "referer", "https://www.huya.com/"}
            };

            var result = await HttpUtil.GetUtf8String($"https://search.cdn.huya.com/?m=Search&do=getSearchContent&q={Uri.EscapeDataString(keyword)}&uid=0&v=4&typ=-5&livestate=0&rows=20&start={(page - 1) * 20}", headers);
            var obj = JObject.Parse(result);

            foreach (var item in obj["response"]["3"]["docs"])
            {
                var cover = item["game_screenshot"].ToString();
                if (!cover.Contains("?"))
                {
                    cover += "?x-oss-process=style/w338_h190&";
                }
                searchResult.Rooms.Add(new LiveRoomItem()
                {
                    Cover = cover,
                    Online = item["game_total_count"].ToInt32(),
                    RoomID = item["room_id"].ToString(),
                    Title = item["game_roomName"].ToString(),
                    UserName = item["game_nick"].ToString(),
                });
            }
            searchResult.HasMore = obj["response"]["3"]["numFound"].ToInt32() > (page * 20);
            return searchResult;
        }
        public Task<List<LivePlayQuality>> GetPlayQuality(LiveRoomDetail roomDetail)
        {
            List<LivePlayQuality> qualities = new List<LivePlayQuality>();
            var urlData = roomDetail.Data as HuyaUrlDataModel;
            if (urlData.BitRates.Count == 0)
            {
                urlData.BitRates = new List<HuyaBitRateModel>() {
                    new HuyaBitRateModel()
                    {
                        Name="原画",
                        BitRate=0,
                    },
                    new HuyaBitRateModel()
                    {
                        Name="高清",
                        BitRate=2000
                    },
                };
            }
            //if (urlData.Lines.Count == 0)
            //{
            //    urlData.Lines = new List<HuyaLineModel>() {
            //        new HuyaLineModel()
            //        {
            //            Line="tx.flv.huya.com",
            //            LineType= HuyaLineType.FLV
            //        },
            //        new HuyaLineModel()
            //        {
            //            Line="bd.flv.huya.com",
            //            LineType= HuyaLineType.FLV
            //        },
            //        new HuyaLineModel()
            //        {
            //            Line="al.flv.huya.com",
            //            LineType= HuyaLineType.FLV
            //        },
            //        new HuyaLineModel()
            //        {
            //            Line="hw.flv.huya.com",
            //            LineType= HuyaLineType.FLV
            //        },
            //    };
            //}
            //var url = GetRealUrl(urlData.Url);

            foreach (var item in urlData.BitRates)
            {
                //var urls = new List<string>();
                //foreach (var line in urlData.Lines)
                //{
                //    var src = line.Line;

                //    src += $"/{line.StreamName}";
                //    if (line.LineType == HuyaLineType.FLV)
                //    {
                //        src += ".flv";
                //    }
                //    if (line.LineType == HuyaLineType.HLS)
                //    {
                //        src += ".m3u8";
                //    }

                //    var param = ProcessAnticode(line.LineType == HuyaLineType.FLV ? line.FlvAntiCode : line.HlsAntiCode, urlData.Uid, line.StreamName);

                //    src += $"?{param}";

                //    if (item.BitRate > 0)
                //    {
                //        src = $"{src}&ratio={item.BitRate}";
                //    }
                //    urls.Add(src);
                //}

                qualities.Add(new LivePlayQuality()
                {
                    Data = new HuyaQualityData()
                    {
                        BitRate = item.BitRate,
                        Lines = urlData.Lines,
                    },
                    Quality = item.Name,
                });
            }




            return Task.FromResult(qualities);
        }
        public string ProcessAnticode(string anticode, string uid, string streamname)
        {
            // https://github.com/iceking2nd/real-url/blob/master/huya.py
            var query = HttpUtility.ParseQueryString(anticode);
            query["t"] = "103";
            query["ctype"] = "tars_mobile";
            var wsTime = (Utils.GetTimestamp() + 21600).ToString("x");
            var seqId = (Utils.GetTimestampMs() + long.Parse(uid)).ToString();
            var fm = Encoding.UTF8.GetString(Convert.FromBase64String(Uri.UnescapeDataString(query["fm"])));
            var wsSecretPrefix = fm.Split('_').First();
            var wsSecretHash = Utils.ToMD5($"{seqId}|{query["ctype"]}|{query["t"]}");
            var wsSecret = Utils.ToMD5($"{wsSecretPrefix}_{uid}_{streamname}_{wsSecretHash}_{wsTime}");


            var map = new NameValueCollection();
            map.Add("wsSecret", wsSecret);
            map.Add("wsTime", wsTime);
            map.Add("seqid", seqId);
            map.Add("ctype", query["ctype"]);
            map.Add("ver", "1");
            map.Add("fs", query["fs"]);
            //map.Add("sphdcdn", query["sphdcdn"] ?? "");
            //map.Add("sphdDC", query["sphdDC"] ?? "");
            //map.Add("sphd", query["sphd"] ?? "");
            //map.Add("exsphd", query["exsphd"] ?? "");
            map.Add("uid", uid);
            map.Add("uuid", GetUuid().ToString());
            map.Add("t", query["t"]);
            map.Add("sv", "202411221719");

            map.Add("dMod", "mseh-0");
            map.Add("sdkPcdn", "1_1");
            map.Add("sdk_sid", "1732862566708");
            map.Add("a_block", "0");


            //将map转为字符串
            var param = string.Join("&", map.AllKeys.Select(x => $"{x}={Uri.EscapeDataString(map[x])}"));
            return param;
        }

        public async Task<List<string>> GetPlayUrls(LiveRoomDetail roomDetail, LivePlayQuality qn)
        {
            var data = qn.Data as HuyaQualityData;
            var urls = new List<string>();
            foreach (var line in data.Lines)
            {
                urls.Add(await GetRealUrl(line, data.BitRate));
            }

            return urls;
        }

        private async Task<string> GetRealUrl(HuyaLineModel line, int bitrate)
        {
            HYGetCdnTokenReq req = new HYGetCdnTokenReq();
            req.stream_name = line.StreamName;
            req.cdn_type = line.CdnType;

            var resp = await tupHttpHelper.GetAsync(req, "getCdnTokenInfo", new HYGetCdnTokenResp());
            var url =$"{line.Line}/{resp.stream_name}.flv?{resp.flv_anti_code}&codec=264";
            if (bitrate > 0)
            {
                url += $"&ratio={bitrate}";
            }
            return url;
        }

        public async Task<bool> GetLiveStatus(object roomId)
        {
            var roomInfo = await GetRoomInfo(roomId.ToString());
            return roomInfo["roomInfo"]["eLiveStatus"].ToInt32() == 2;
        }
        public Task<List<LiveSuperChatMessage>> GetSuperChatMessages(object roomId)
        {
            return Task.FromResult(new List<LiveSuperChatMessage>());
        }
    }
    public class HuyaUrlDataModel
    {
        public string Url { get; set; }
        public string Uid { get; set; }
        public long UUid { get; set; }
        public List<HuyaLineModel> Lines { get; set; }
        public List<HuyaBitRateModel> BitRates { get; set; }
    }
    public enum HuyaLineType
    {
        FLV = 0,
        HLS = 1,
    }
    public class HuyaLineModel
    {
        public string Line { get; set; }
        public string FlvAntiCode { get; set; }
        public string StreamName { get; set; }
        public string HlsAntiCode { get; set; }
        public string CdnType { get; set; }
        public HuyaLineType LineType { get; set; }
    }
    public class HuyaBitRateModel
    {
        public string Name { get; set; }
        public int BitRate { get; set; }

    }
    public class HuyaQualityData
    {
        public int BitRate { get; set; }
        public List<HuyaLineModel> Lines { get; set; }
    }
}
