using System;
using System.Collections.Generic;
using System.Text;

namespace AllLive.Core.Models
{
    public class LiveCategoryResult
    {
        /// <summary>
        /// 是否有更多结果
        /// </summary>
        public bool HasMore { get; set; }
        /// <summary>
        /// 搜索结果
        /// </summary>
        public List<LiveRoomItem> Rooms { get; set; }
    }
}
