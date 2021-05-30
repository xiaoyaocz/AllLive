using AllLive.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.ViewModels
{
    public class MainVM
    {
       
        public static List<Site> Sites = new List<Site>() { 
            new Site()
            {
                Name="哔哩哔哩直播",
                Logo="ms-appx:///Assets/Logo/bilibili.png",
                LiveSite=new AllLive.Core.BiliBili(),
            },
            new Site()
            {
                Name="斗鱼直播",
                Logo="ms-appx:///Assets/Logo/douyu.png",
                LiveSite=new AllLive.Core.Douyu(),
            },
            new Site()
            {
                Name="虎牙直播",
                Logo="ms-appx:///Assets/Logo/huya.png",
                LiveSite=new AllLive.Core.Huya(),
            },
            //new Site()
            //{
            //    Name="企鹅电竞",
            //    Logo="ms-appx:///Assets/Logo/egame.png",
            //    LiveSite=new AllLive.Core.Egame(),
            //},
        };
        
    }
    public class Site
    {
        public string Name { get; set; }
        public ILiveSite LiveSite { get; set; }
        public string Logo { get; set; }
    }
}
