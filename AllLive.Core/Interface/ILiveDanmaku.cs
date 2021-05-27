using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.Core.Interface
{
    public interface ILiveDanmaku
    {
        /// <summary>
        /// 新信息事件
        /// </summary>
        event EventHandler<LiveMessage> NewMessage;
        event EventHandler<string> OnClose;
        /// <summary>
        /// 发送心跳包间隔时间/毫秒
        /// </summary>
        int HeartbeatTime { get; }
        /// <summary>
        /// 发送心跳包
        /// </summary>
        void Heartbeat();
        /// <summary>
        /// 开始接收弹幕
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Start(object args);
        /// <summary>
        /// 停止接收弹幕
        /// </summary>
        /// <returns></returns>
        Task Stop();
    }
}
