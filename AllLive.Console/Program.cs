using AllLive.Core;
using AllLive.Core.Danmaku;
using AllLive.Core.Helper;
using AllLive.Core.Interface;
using Newtonsoft.Json;
using QuickJS;
using System;
using System.Text.RegularExpressions;

namespace AllLive.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("输入错误");
                return;
            }
           
            var action = args[0];
            
            try
            {
                if (args.Length==1)
                {
                    if(action == "h" || action == "-h" || action == "?")
                    {
                        Console.WriteLine("-i [URL] ：获取直播间信息及播放直链");
                        Console.WriteLine("-d [URL] ：持续输出直播间弹幕");
                    }
                    else
                    {
                        Console.WriteLine($"未知指令：{action}");
                    }
                    return;
                }
                var url = args[1];
                var parseData = ParseUrl(url);
                var site = parseData.Item1;
                var roomId = parseData.Item2;
                if (action == "i"|| action == "-i")
                {
                    var detail = site.GetRoomDetail(roomId).Result;
                    Console.WriteLine($"来源：{site.Name}");
                    Console.WriteLine($"房间号：{detail.RoomID}");
                    Console.WriteLine($"房间标题：{detail.Title}");
                    Console.WriteLine($"直播用户：{detail.UserName}");
                    Console.WriteLine($"人气值：{detail.Online}");
                    Console.WriteLine($"可用清晰度：");
                    var quality= site.GetPlayQuality(detail).Result;
                   
                    for (int i = 0; i < quality.Count; i++)
                    {
                        Console.WriteLine($"【{i}】{quality[i].Quality}");
                    }
                    Console.WriteLine($"请输入【】内数字，获取对应清晰度的直链");
                    var input = Console.ReadLine();
                    Console.WriteLine($"正在获取直链...");
                    if (int.TryParse(input,out var index))
                    {
                        var urls= site.GetPlayUrls(detail, quality[index]).Result;
                        for (int i = 0; i < urls.Count; i++)
                        {
                            Console.WriteLine($"线路{i+1}:\r\n{urls[i]}");
                        }
                    }
                }
                else if (action == "d"||action == "-d")
                {
                    Console.WriteLine($"正在获取房间信息...");
                    var detail = site.GetRoomDetail(roomId).Result;
                    Console.WriteLine($"房间标题：{detail.Title}");
                    Console.WriteLine($"直播用户：{detail.UserName}");
                    var danmaku = site.GetDanmaku();
                    danmaku.NewMessage += Danmaku_NewMessage;
                    danmaku.OnClose += Danmaku_OnClose;
                    Console.WriteLine($"【开始获取弹幕，按任意键结束】");
                    danmaku.Start(detail.DanmakuData).Wait();
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine($"未知指令：{action}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        private static (ILiveSite, string) ParseUrl(string url)
        {
            if (url.Contains("bilibili.com"))
            {
                var id = Regex.Match(url, @"bilibili\.com/([\d|\w]+)").Groups[1].Value;
                return (new BiliBili(), id);
            }
            if (url.Contains("huya.com"))
            {
                var id = Regex.Match(url, @"huya\.com/([\d|\w]+)").Groups[1].Value;
                return (new Huya(), id);
            }
            if (url.Contains("douyu.com"))
            {
                var id = Regex.Match(url, @"douyu\.com/([\d|\w]+)").Groups[1].Value;
                return (new Douyu(), id);
            }
            throw new Exception("链接解析失败");
        }

        private static void Danmaku_OnClose(object sender, string e)
        {
            Console.WriteLine("------关闭连接-----");
        }

        private static void Danmaku_NewMessage(object sender, Core.Models.LiveMessage e)
        {
            if (e.Type == Core.Models.LiveMessageType.Chat)
            {
                Console.WriteLine($"[{e.UserName}]：{e.Message}");
            }
            if (e.Type == Core.Models.LiveMessageType.Online)
            {
                Console.WriteLine($"------人气值：{e.Data}-----");
            }
        }
    }
}
