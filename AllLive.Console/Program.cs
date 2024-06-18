using AllLive.Core;
using AllLive.Core.Danmaku;
using AllLive.Core.Helper;
using AllLive.Core.Interface;
using AllLive.UWP.Helper;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AllLive.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            do
            {
                Console.Clear();
                if (args.Length == 0)
                {
                    Console.WriteLine("请选择要执行的操作：");
                    Console.WriteLine("【1】获取直播间直链");
                    Console.WriteLine("【2】实时输出弹幕");
                    Console.WriteLine("【3】退出程序");
                    Console.Write("操作序号：");
                    var input = Console.ReadLine();

                    if (input == "3")
                    {
                        break;
                    }

                    if (input != "1" && input != "2")
                    {
                        continue;
                    }
                    Console.Clear();
                    Console.WriteLine("请输入直播间链接，支持哔哩哔哩、斗鱼直播、虎牙直播、抖音直播");
                    Console.Write("URL：");
                    var url = Console.ReadLine();
                    args = new string[] { input == "1" ? "-i" : "-d", url };
                    Console.Clear();
                }
                try
                {
                    await Action(args);
                    args = new string[0];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    break;
                }



            } while (true);

        }

        private static async Task Action(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("参数有误,详细参数信息请通过-h查看");
                return;
            }


            var action = args[0];

            if (args.Length == 1)
            {
                if (action == "h" || action == "-h" || action == "?")
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
            var parseData = await SiteParser.ParseUrl(url);
            ILiveSite site = null;
            switch (parseData.Item1)
            {
                case LiveSite.Bilibili:
                    site = new BiliBili();
                    break;
                case LiveSite.Douyu:
                    site = new Douyu();
                    break;
                case LiveSite.Huya:
                    site = new Huya();
                    break;
                case LiveSite.Douyin:
                    site = new Douyin();
                    break;
                case LiveSite.Unknown:
                    Console.WriteLine("未知直播源");
                    return;
                default:
                    break;
            }

            var roomId = parseData.Item2;
            if (action == "i" || action == "-i")
            {
                Console.WriteLine($"正在获取房间信息...");
                var detail = await site.GetRoomDetail(roomId);
                Console.WriteLine($"来源：{site.Name}");
                Console.WriteLine($"房间号：{detail.RoomID}");
                Console.WriteLine($"房间标题：{detail.Title}");
                Console.WriteLine($"直播用户：{detail.UserName}");
                Console.WriteLine($"人气值：{detail.Online}");
                Console.Write("状态：");
                Console.ForegroundColor = detail.Status ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(detail.Status ? "直播中" : "未开播");
                Console.ResetColor();
                if (detail.Status)
                {
                    Console.WriteLine($"可用清晰度：");
                    var quality = await site.GetPlayQuality(detail);

                    for (int i = 0; i < quality.Count; i++)
                    {
                        Console.WriteLine($"【{i}】{quality[i].Quality}");
                    }
                    Console.Write($"请输入清晰度序号：");
                    var input = Console.ReadLine();
                    Console.WriteLine($"正在获取直链...");
                    if (int.TryParse(input, out var index))
                    {
                        var urls = await site.GetPlayUrls(detail, quality[index]);
                        for (int i = 0; i < urls.Count; i++)
                        {
                            Console.WriteLine($"线路{i + 1}：{urls[i]}");
                        }
                    }
                    Console.WriteLine($"按任意键结束");
                    Console.ReadKey();
                }

            }
            else if (action == "d" || action == "-d")
            {
                Console.WriteLine($"正在获取房间信息...");
                var detail = await site.GetRoomDetail(roomId);
                Console.WriteLine($"房间标题：{detail.Title}");
                Console.WriteLine($"直播用户：{detail.UserName}");
                Console.Write("直播状态：");
                Console.ForegroundColor = detail.Status ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(detail.Status ? "直播中" : "未开播");
                Console.ResetColor();
                var danmaku = site.GetDanmaku();
                danmaku.NewMessage += Danmaku_NewMessage;
                danmaku.OnClose += Danmaku_OnClose;
                Console.WriteLine($"【开始获取弹幕，按任意键结束】");
                danmaku.Start(detail.DanmakuData).Wait();
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine($"未知指令：{action}");
            }
        }


        private static void Danmaku_OnClose(object sender, string e)
        {
            Console.WriteLine("------关闭连接-----");
        }

        private static void Danmaku_NewMessage(object sender, Core.Models.LiveMessage e)
        {
            if (e.Type == Core.Models.LiveMessageType.Chat)
            {
                Console.ForegroundColor = ClosestConsoleColor(e.Color.R, e.Color.G, e.Color.B);
                Console.WriteLine($"[{e.UserName}]：{e.Message}");
                Console.ResetColor();
            }
            if (e.Type == Core.Models.LiveMessageType.Online)
            {
                Console.WriteLine($"------人气值：{e.Data}-----");
            }
        }
        static ConsoleColor ClosestConsoleColor(byte r, byte g, byte b)
        {
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }
}
