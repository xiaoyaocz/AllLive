using System;
using System.Collections.Generic;
using System.Text;

namespace AllLive.Core.Models
{
    public class LiveCategory
    {
        public string Name { get; set; }
        public string ID{ get; set; }
        public List<LiveSubCategory> Children { get; set; }
}
    public class LiveSubCategory
    {
        public string Name { get; set; }
        public string Pic { get; set; }
        public string ID { get; set; }
        public string ParentID{ get; set; }
    }
}
