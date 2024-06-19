using AllLive.Core.Helper;
using AllLive.Core.Interface;
using AllLive.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using WebSocketSharp;
/*
* 哔哩哔哩弹幕实现
* 参考文档：https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md
*/
namespace AllLive.Core.Danmaku
{
    public class BiliDanmakuArgs
    {
        public int RoomId { get; set; }
        public long UserId { get; set; } = 0;
        public string Cookie { get; set; }
    }
    public class BiliBiliDanmaku : ILiveDanmaku
    {
        public event EventHandler<LiveMessage> NewMessage;
        public event EventHandler<string> OnClose;
        public int HeartbeatTime => 60 * 1000;
        private int roomId = 0;
        //private readonly string ServerUrl = "wss://broadcastlv.chat.bilibili.com/sub";
        Timer timer;
        WebSocket ws;
        private DanmuInfo danmuInfo;
        private string buvid;
        private BiliDanmakuArgs Args;

        public BiliBiliDanmaku()
        {

        }
        private async void Ws_OnOpen(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                //发送进房信息
                ws.Send(EncodeData(JsonConvert.SerializeObject(new
                {
                    roomid = roomId,
                    uid =Args.UserId,
                    protover = 2,
                    key = danmuInfo.token,
                    platform = "web",
                    type=2,
                    buvid,
                }), 7));

            });
            timer.Start();

        }
        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                ParseData(e.RawData);
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

        public async Task Start(object args)
        {
            var _args = args as BiliDanmakuArgs;
            Args = _args;
            roomId = Args.RoomId;

            var _buvid = await GetBuvid();
            buvid = _buvid;
            var info = await GetDanmuInfo(roomId);
            if (info == null)
            {
                SendSystemMessage("获取弹幕信息失败");
                return;
            }
            danmuInfo = info;
            var host = info.host_list.First();
            ws = new WebSocket($"wss://{host.host}/sub");
            if (!string.IsNullOrEmpty(Args.Cookie))
            {
                ws.CustomHeaders = new Dictionary<string, string>() {
                    {"Cookie", Args.Cookie},
                };
            }

            ws.OnOpen += Ws_OnOpen;
            ws.OnError += Ws_OnError;
            ws.OnMessage += Ws_OnMessage;
            ws.OnClose += Ws_OnClose;
            timer = new Timer(HeartbeatTime);
            timer.Elapsed += Timer_Elapsed;
            await Task.Run(() =>
            {
                ws.Connect();
            });
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Heartbeat();
        }

        public async void Heartbeat()
        {
            await Task.Run(() =>
            {
                ws.Send(EncodeData("", 2));
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

        private void ParseData(byte[] data)
        {
            //协议版本。
            //0为JSON，可以直接解析；
            //1为房间人气值,Body为Int32；
            //2为zlib压缩过Buffer，需要解压再处理
            //3为brotli压缩过Buffer，需要解压再处理
            int protocolVersion = BitConverter.ToInt32(new byte[4] { data[7], data[6], 0, 0 }, 0);
            //操作类型。
            //3=心跳回应，内容为房间人气值；
            //5=通知，弹幕、广播等全部信息；
            //8=进房回应，空
            int operation = BitConverter.ToInt32(data.Skip(8).Take(4).Reverse().ToArray(), 0);

            //内容
            var body = data.Skip(16).ToArray();
            if (operation == 3)
            {
                var online = BitConverter.ToInt32(body.Reverse().ToArray(), 0);
                NewMessage?.Invoke(this, new LiveMessage()
                {
                    Data = online,
                    Type = LiveMessageType.Online,
                });
            }
            else if (operation == 5)
            {

                if (protocolVersion == 2)//|| protocolVersion == 3
                {
                    body = DecompressData(body, protocolVersion);
                }
                var text = Encoding.UTF8.GetString(body);
                //可能有多条数据，做个分割
                var textLines = Regex.Split(text, "[\x00-\x1f]+").Where(x => x.Length > 2 && x[0] == '{').ToArray();
                foreach (var item in textLines)
                {
                    ParseMessage(item);
                }
            }
        }

        private void ParseMessage(string jsonMessage)
        {
            try
            {
                var obj = JObject.Parse(jsonMessage);
                var cmd = obj["cmd"].ToString();
                if (cmd.Contains("DANMU_MSG"))
                {
                    if (obj["info"] != null && obj["info"].ToArray().Length != 0)
                    {
                        var message = obj["info"][1].ToString();
                        var color = obj["info"][0][3].ToInt32();
                        if (obj["info"][2] != null && obj["info"][2].ToArray().Length != 0)
                        {
                            var username = obj["info"][2][1].ToString();
                            NewMessage?.Invoke(this, new LiveMessage()
                            {
                                Type = LiveMessageType.Chat,
                                Message = message,
                                UserName = username,
                                Color = color == 0 ? DanmakuColor.White : new DanmakuColor(color),
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// 对数据进行编码
        /// </summary>
        /// <param name="msg">文本内容</param>
        /// <param name="action">2=心跳，7=进房</param>
        /// <returns></returns>
        private byte[] EncodeData(string msg, int action)
        {
            var data = Encoding.UTF8.GetBytes(msg);
            //头部长度固定16
            var length = data.Length + 16;
            var buffer = new byte[length];
            using (var ms = new MemoryStream(buffer))
            {

                //数据包长度
                var b = BitConverter.GetBytes(buffer.Length).ToArray().Reverse().ToArray();
                ms.Write(b, 0, 4);
                //数据包头部长度,固定16
                b = BitConverter.GetBytes(16).Reverse().ToArray();
                ms.Write(b, 2, 2);
                //协议版本，0=JSON,1=Int32,2=Buffer
                b = BitConverter.GetBytes(0).Reverse().ToArray(); ;
                ms.Write(b, 0, 2);
                //操作类型
                b = BitConverter.GetBytes(action).Reverse().ToArray(); ;
                ms.Write(b, 0, 4);
                //数据包头部长度,固定1
                b = BitConverter.GetBytes(1).Reverse().ToArray(); ;
                ms.Write(b, 0, 4);
                //数据
                ms.Write(data, 0, data.Length);
                var _bytes = ms.ToArray();
                ms.Flush();
                return _bytes;
            }

        }


        /// <summary>
        /// 解码数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] DecompressData(byte[] data, int protocolVersion)
        {
            if (protocolVersion == 3)
            {
                return DecompressDataWithBrotli(data);
            }
            using (MemoryStream outBuffer = new MemoryStream())
            using (System.IO.Compression.DeflateStream compressedzipStream = new System.IO.Compression.DeflateStream(new MemoryStream(data, 2, data.Length - 2), System.IO.Compression.CompressionMode.Decompress))
            {
                byte[] block = new byte[1024];
                while (true)
                {
                    int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                    if (bytesRead <= 0)
                        break;
                    else
                        outBuffer.Write(block, 0, bytesRead);
                }
                compressedzipStream.Close();
                return outBuffer.ToArray();
            }


        }
        /// <summary>
        /// 解压数据 (使用Brotli)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] DecompressDataWithBrotli(byte[] data)
        {
            
            //using (var decompressedStream = new BrotliStream(new MemoryStream(data), CompressionMode.Decompress))
            //{
            //    using (var outBuffer = new MemoryStream())
            //    {
            //        var block = new byte[1024];
            //        while (true)
            //        {
            //            var bytesRead = decompressedStream.Read(block, 0, block.Length);
            //            if (bytesRead <= 0)
            //                break;
            //            outBuffer.Write(block, 0, bytesRead);
            //        }
            //        return outBuffer.ToArray();
            //    }
            //}
            throw new NotImplementedException();


        }
        private async Task<string> GetBuvid()
        {
            try
            {
                if (!string.IsNullOrEmpty(Args.Cookie) &&Args.Cookie.Contains("buvid3"))
                {
                    var regex = new Regex("buvid3=(.*?);");
                    var match = regex.Match(Args.Cookie);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }

                var result = await HttpUtil.GetString($"https://api.bilibili.com/x/frontend/finger/spi",
                    headers: string.IsNullOrEmpty(Args.Cookie) ? null : new Dictionary<string, string>
                    {
                        { "cookie", Args.Cookie }
                    }
                  );
                var obj = JObject.Parse(result);

                return obj["data"]["b_3"].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private async Task<DanmuInfo> GetDanmuInfo(int roomId)
        {
            try
            {
                var result = await HttpUtil.GetString($"https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo?id={roomId}",
                    headers: string.IsNullOrEmpty(Args.Cookie) ? null : new Dictionary<string, string>
                    {
                        { "cookie", Args.Cookie }
                    });
                var obj = JObject.Parse(result);
                var info = obj["data"].ToObject<DanmuInfo>();
                return info;
            }
            catch (Exception ex)
            {
                SendSystemMessage(ex.Message);
            }
            return null;
        }

        private void SendSystemMessage(string msg)
        {
            NewMessage(this, new LiveMessage()
            {
                Type = LiveMessageType.Chat,
                UserName = "系统",
                Message = msg
            });
        }
    }



    class DanmuInfo
    {
        public string group { get; set; }
        public int business_id { get; set; }
        public double refresh_row_factor { get; set; }
        public int refresh_rate { get; set; }
        public int max_delay { get; set; }
        public string token { get; set; }
        public List<DanmuInfoHostList> host_list { get; set; }
    }

    class DanmuInfoHostList
    {
        public string host { get; set; }
        public int port { get; set; }
        public int wss_port { get; set; }
        public int ws_port { get; set; }
    }
}
