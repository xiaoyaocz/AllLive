using AllLive.Core.Danmaku;
using AllLive.Core.Interface;
using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.Core
{
    public class Egame : ILiveSite
    {
        public string Name => "企鹅电竞";
        public ILiveDanmaku GetDanmaku() => new EgameDanmaku();
        public Task<List<LiveCategory>> GetCategores()
        {
            throw new NotImplementedException();
        }

        public Task<LiveCategoryResult> GetCategoryRooms(LiveSubCategory category, int page = 1)
        {
            throw new NotImplementedException();
        }

       

        public Task<List<LivePlayQuality>> GetPlayQuality(LiveRoomDetail roomDetail)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetPlayUrls(LiveRoomDetail roomDetail, LivePlayQuality qn)
        {
            throw new NotImplementedException();
        }

        public Task<LiveCategoryResult> GetRecommendRooms(int page = 1)
        {
            throw new NotImplementedException();
        }

        public Task<LiveRoomDetail> GetRoomDetail(object roomId)
        {
            throw new NotImplementedException();
        }

        public Task<LiveSearchResult> Search(string keyword, int page = 1)
        {
            throw new NotImplementedException();
        }
    }
}
