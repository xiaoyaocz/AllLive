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
        public Color Color { get; set; }= Color.White;
    }
}
