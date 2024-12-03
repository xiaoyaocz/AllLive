using AllLive.Core.Helper;
using AllLive.Core.Interface;
using AllLive.Core.Models;
using AllLive.Core.Models.Tars;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Tup.Tars;
using WebSocketSharp;


/*
* 虎牙弹幕实现
* 参考项目：
* https://github.com/BacooTang/huya-danmu
* https://github.com/IsoaSFlus/danmaku
*/
namespace AllLive.Core.Danmaku
{
    public class HuyaDanmakuArgs
    {
        public HuyaDanmakuArgs(long ayyuid, long topSid, long subSid)
        {
            this.Ayyuid = ayyuid;
            this.SubSid = subSid;
            this.TopSid = topSid;
        }
        public long Ayyuid { get; set; }
        public long TopSid { get; set; }
        public long SubSid { get; set; }
    }
    public class HuyaDanmaku : ILiveDanmaku
    {
        public int HeartbeatTime => 60 * 1000;

        public event EventHandler<LiveMessage> NewMessage;
        public event EventHandler<string> OnClose;
        readonly byte[] heartBeatData;
        private readonly string ServerUrl = "wss://cdnws.api.huya.com";
        Timer timer;
        WebSocket ws;
        HuyaDanmakuArgs args;
        public HuyaDanmaku()
        {
            heartBeatData = Convert.FromBase64String("ABQdAAwsNgBM");
            ws = new WebSocket(ServerUrl);
            ws.OnOpen += Ws_OnOpen;
            ws.OnError += Ws_OnError;
            ws.OnMessage += Ws_OnMessage;
            ws.OnClose += Ws_OnClose;
            timer = new Timer(HeartbeatTime);
            timer.Elapsed += Timer_Elapsed;

        }
        private async void Ws_OnOpen(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                //发送进房信息
                ws.Send(JoinData(args.Ayyuid, args.TopSid, args.SubSid));

            });
            timer.Start();

        }
        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var stream = new TarsInputStream(e.RawData);
                var type = stream.Read(0, 0, false);
                if (type == 7)
                {
                    stream = new TarsInputStream(stream.Read(new byte[0], 1, false));
                    HYPushMessage wSPushMessage = new HYPushMessage();
                    wSPushMessage.ReadFrom(stream);
                    if (wSPushMessage.Uri == 1400)
                    {

                        HYMessage messageNotice = new HYMessage();
                        messageNotice.ReadFrom(new TarsInputStream(wSPushMessage.Msg));
                        var uname = messageNotice.UserInfo.NickName;
                        var content = messageNotice.Content;
                        var color = messageNotice.BulletFormat.FontColor;
                        NewMessage?.Invoke(this, new LiveMessage()
                        {
                            Type = LiveMessageType.Chat,
                            Message = content,
                            UserName = uname,
                            Color = color <= 0 ? DanmakuColor.White : new DanmakuColor(color),
                        });

                    }
                    if (wSPushMessage.Uri == 8006)
                    {
                        long online = 0;
                        var s = new TarsInputStream(wSPushMessage.Msg);
                        online = s.Read(online, 0, false);
                        NewMessage?.Invoke(this, new LiveMessage()
                        {
                            Type = LiveMessageType.Online,
                            Data = online,
                        });
                    }
                }
                else if (type == 22)
                {
                    Debug.WriteLine($"收到消息:[Type:{type}]");
                    stream = new TarsInputStream(stream.Read(new byte[0], 1, false));
                    HYPushMessageV2 wSPushMessage = new HYPushMessageV2();
                    wSPushMessage.ReadFrom(stream);
                    foreach (var item in wSPushMessage.MsgItem)
                    {
                        if (item.Uri == 1400)
                        {
                            HYMessage messageNotice = new HYMessage();
                            messageNotice.ReadFrom(new TarsInputStream(item.Msg));
                            var uname = messageNotice.UserInfo.NickName;
                            var content = messageNotice.Content;
                            var color = messageNotice.BulletFormat.FontColor;
                            NewMessage?.Invoke(this, new LiveMessage()
                            {
                                Type = LiveMessageType.Chat,
                                Message = content,
                                UserName = uname,
                                Color = color <= 0 ? DanmakuColor.White : new DanmakuColor(color),
                            });

                        }
                        if (item.Uri == 8006)
                        {
                            long online = 0;
                            var s = new TarsInputStream(item.Msg);
                            online = s.Read(online, 0, false);
                            NewMessage?.Invoke(this, new LiveMessage()
                            {
                                Type = LiveMessageType.Online,
                                Data = online,
                            });
                        }
                    }
                  
                }
            }
            catch (Exception)
            {
            }
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            OnClose?.Invoke(this, e.Reason);
        }

        private void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            OnClose?.Invoke(this, e.Message);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Heartbeat();
        }

        public async void Heartbeat()
        {
            await Task.Run(() =>
            {
                ws.Send(heartBeatData);
            });
        }

        public async Task Start(object args)
        {
            this.args = (HuyaDanmakuArgs)args;
            await Task.Run(() =>
            {
                ws.Connect();
            });
        }

        public async Task Stop()
        {
            timer.Stop();
            await Task.Run(() =>
            {
                ws.Close();
            });
        }

        private byte[] JoinData(long ayyuid, long tid, long sid)
        {
            var oos = new TarsOutputStream();
            oos.Write(ayyuid, 0);
            oos.Write(true, 1);
            oos.Write("", 2);
            oos.Write("", 3);
            oos.Write(tid, 4);
            oos.Write(sid, 5);
            oos.Write(0, 6);
            oos.Write(0, 7);

            var wscmd = new TarsOutputStream();
            wscmd.Write(1, 0);
            wscmd.Write(oos.toByteArray(), 1);
            return wscmd.toByteArray();


        }
    }



}
