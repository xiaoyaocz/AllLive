using System;
using System.Collections.Generic;
using System.Text;

namespace AllLive.Core.Models
{
    public class LiveRoomDetail
    {
        /// <summary>
        /// 房间号
        /// </summary>
        public string RoomID { get; set; }
        /// <summary>
        /// 房间标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 封面
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string UserAvatar { get; set; }
        /// <summary>
        /// 在线人数/人气
        /// </summary>
        public int Online { get; set; }
        /// <summary>
        /// 房间介绍
        /// </summary>
        public string Introduction { get; set; }
        /// <summary>
        /// 房间公告
        /// </summary>
        public string Notice { get; set; }
        /// <summary>
        /// 直播状态
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// 一些其他信息
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 弹幕数据
        /// </summary>
        public object DanmakuData { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }
    }
}
