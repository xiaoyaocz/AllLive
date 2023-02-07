using AllLive.Core.Helper;
using AllLive.Core.Interface;
using AllLive.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public HuyaDanmakuArgs(long ayyuid,long topSid,long subSid)
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
                            Color= color <= 0 ? Color.White : Utils.NumberToColor(color),
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


    public class HYPushMessage : TarsStruct
    {
        public int PushType = 0;
        public long Uri = 0;
        public byte[] Msg = new byte[0];
        public int ProtocolType = 0;
        public override void ReadFrom(TarsInputStream _is)
        {
            PushType = _is.Read(PushType, 0, false);
            Uri = _is.Read(Uri, 1, false);
            Msg = _is.Read(Msg, 2, false);
            ProtocolType = _is.Read(ProtocolType, 3, false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(PushType, 0);
            _os.Write(Uri, 1);
            _os.Write(Msg, 2);
            _os.Write(ProtocolType, 3);
        }
    }
    public class HYSender : TarsStruct
    {
        public long Uid = 0;
        public long Lmid = 0;
        public string NickName = "";
        public int Gender = 0;

        public override void ReadFrom(TarsInputStream _is)
        {
            Uid = _is.Read(Uid, 0, false);
            Lmid = _is.Read(Lmid, 0, false);
            NickName = _is.Read(NickName, 2, false);
            Gender = _is.Read(Gender, 3, false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(Uid, 0);
            _os.Write(Lmid, 1);
            _os.Write(NickName, 2);
            _os.Write(Gender, 3);
        }
    }
    public class HYMessage : TarsStruct
    {
        public HYSender UserInfo = new HYSender();
        public string Content = "";
        public HYBulletFormat BulletFormat = new HYBulletFormat();
        public override void ReadFrom(TarsInputStream _is)
        {
            UserInfo = (HYSender)_is.Read(UserInfo, 0, false);
            Content = _is.Read(Content, 3, false);
            BulletFormat = (HYBulletFormat)_is.Read(BulletFormat, 6, false);
        }
        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(UserInfo, 0);
            _os.Write(Content, 3);
            _os.Write(BulletFormat, 6);
        }
    }
    public class HYBulletFormat : TarsStruct
    {
        public int FontColor =0;
        public int FontSize = 4;
        public int TextSpeed = 0;
        public int TransitionType = 1;
        public override void ReadFrom(TarsInputStream _is)
        {
            FontColor = _is.Read(FontColor, 0, false);
            FontSize = _is.Read(FontSize, 1, false);
            TextSpeed = _is.Read(TextSpeed, 2, false);
            TransitionType = _is.Read(TransitionType, 3, false);
        }
        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(FontColor, 0);
            _os.Write(FontSize, 1);
            _os.Write(FontSize, 2);
            _os.Write(FontSize, 3);
        }
    }
}
