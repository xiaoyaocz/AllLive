using AllLive.Core.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AllLive.Core.Models
{
    public enum LiveMessageType
    {
        /// <summary>
        /// 聊天
        /// </summary>
        Chat,
        /// <summary>
        /// 礼物,暂时不支持
        /// </summary>
        Gift,
        /// <summary>
        /// 在线人数
        /// </summary>
        Online,
        /// <summary>
        /// 醒目留言
        /// </summary>
        SuperChat
    }
    public class LiveMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public LiveMessageType Type { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 数据
        /// 单Type=Online时，Data为人气值(long)
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 弹幕颜色
        /// </summary>
        public DanmakuColor Color { get; set; }= DanmakuColor.White;
    }

    public class LiveSuperChatMessage
    {
        public string UserName { get; set; }
        public string Face { get; set; }
        public string Message { get; set; }
        public int Price { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string BackgroundColor { get; set; }
        public string BackgroundBottomColor { get; set; }
    }
}
