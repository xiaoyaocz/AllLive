using System;
using System.Collections.Generic;
using System.Text;

namespace AllLive.Core.Models
{
    public class LiveRoomItem
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
        /// 在线人数/人气
        /// </summary>
        public int Online { get; set; }
    }
}
