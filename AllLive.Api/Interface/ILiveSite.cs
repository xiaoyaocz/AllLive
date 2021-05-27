using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.Core.Interface
{
    public interface ILiveSite
    {
        string Name { get; }
        /// <summary>
        /// 弹幕
        /// </summary>
        ILiveDanmaku GetDanmaku();
        /// <summary>
        /// 读取网站的分类
        /// </summary>
        /// <returns></returns>
        Task<List<LiveCategory>> GetCategores();
        /// <summary>
        /// 搜索直播
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page">页数</param>
        /// <returns></returns>
        Task<LiveSearchResult> Search(string keyword, int page = 1);
        /// <summary>
        /// 读取类目下房间
        /// </summary>
        /// <param name="category">类目</param>
        /// <param name="page">页数</param>
        /// <returns></returns>
        Task<LiveCategoryResult> GetCategoryRooms(LiveSubCategory category, int page = 1);
        /// <summary>
        /// 读取推荐直播
        /// </summary>
        /// <param name="page">页数</param>
        /// <returns></returns>
        Task<LiveCategoryResult> GetRecommendRooms(int page = 1);
        /// <summary>
        /// 房间详情
        /// </summary>
        /// <param name="room">房间</param>
        /// <returns></returns>
        Task<LiveRoomDetail> GetRoomDetail(object roomId);
        /// <summary>
        /// 读取播放清晰度
        /// </summary>
        /// <param name="roomDetail">房间详情</param>
        /// <returns></returns>
        Task<List<LivePlayQuality>> GetPlayQuality(LiveRoomDetail roomDetail);

        /// <summary>
        /// 播放地址
        /// </summary>
        /// <param name="roomDetail">房间详情</param>
        /// <param name="qn">清晰度</param>
        /// <returns></returns>
        Task<List<string>> GetPlayUrls(LiveRoomDetail roomDetail, LivePlayQuality qn);

    }
}
