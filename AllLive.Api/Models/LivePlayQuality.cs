using System;
using System.Collections.Generic;
using System.Text;

namespace AllLive.Core.Models
{
    /// <summary>
    /// 播放清晰度
    /// </summary>
    public class LivePlayQuality
    {
        /// <summary>
        /// 清晰度
        /// </summary>
        public string Quality { get; set; }
        /// <summary>
        /// 清晰度信息
        /// </summary>
        public object Data { get; set; }
    }
}
