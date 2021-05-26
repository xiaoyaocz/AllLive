using AllLive.Core;
using AllLive.Core.Danmaku;
using AllLive.Core.Interface;
using Newtonsoft.Json;
using System;

namespace AllLive.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Huya biliBili = new Huya();
           // var result= biliBili.GetCategores().Result;
           // var result2 = biliBili.GetCategoryRooms(result[0].Children[0]).Result;
            var result3 = biliBili.GetRecommendRooms().Result;
            var result4 = biliBili.GetRoomDetail(result3.Rooms[0]).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result4));

            foreach (var item in biliBili.GetPlayQuality(result4).Result)
            {
                Console.WriteLine($"清晰度 {item.Quality}：");
                var urls = biliBili.GetPlayUrls(result4, item).Result;
                foreach (var item2 in urls)
                {
                    Console.WriteLine(item2);
                }
            }
            //  var result5 = biliBili.Search("王者").Result;
            //BiliBili
            //ILiveDanmaku danmaku = new Core.Danmaku.BiliBiliDanmaku();
            //ILiveDanmaku danmaku = new HuyaDanmaku();
            //ILiveDanmaku danmaku = new DouyuDanmaku();
            //danmaku.NewMessage += Danmaku_NewMessage;
            // danmaku.OnClose += Danmaku_OnClose;

            //danmaku.Start(6154037);
            //danmaku.Start(new HuyaDanmakuArgs(1199515480194, 1199515480194, 1199515480194));
            // danmaku.Start(101).Wait();
            Console.ReadLine();
            //danmaku.Stop().Wait();
           
        }

        private static void Danmaku_OnClose(object sender, string e)
        {
            Console.WriteLine("------关闭连接-----");
        }

        private static void Danmaku_NewMessage(object sender, Core.Models.LiveMessage e)
        {
            if(e.Type== Core.Models.LiveMessageType.Chat)
            {
                Console.WriteLine($"{e.UserName}:{e.Message}");
            }
            if (e.Type == Core.Models.LiveMessageType.Online)
            {
                Console.WriteLine($"------人气值：{e.Data}-----");
            }
        }
    }
}
