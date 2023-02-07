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
using WebSocketSharp;
/*
* 哔哩哔哩弹幕实现
* 参考文档：https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md
*/
namespace AllLive.Core.Danmaku
{

    public class BiliBiliDanmaku : ILiveDanmaku
    {
        public event EventHandler<LiveMessage> NewMessage;
        public event EventHandler<string> OnClose;
        public int HeartbeatTime => 60 * 1000;
        private int roomId = 0;
        private readonly string ServerUrl = "wss://broadcastlv.chat.bilibili.com/sub";
        Timer timer;
        WebSocket ws;
        public BiliBiliDanmaku()
        {
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
                ws.Send(EncodeData(JsonConvert.SerializeObject(new
                {
                    roomid = roomId,
                    uid = 0
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
            roomId = args.ToInt32();
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
            await Task.Run(() =>
            {
                ws.Close();
            });
        }

        private void ParseData(byte[] data)
        {
            //协议版本。0为JSON，可以直接解析；1为房间人气值,Body为4位Int32；2为压缩过Buffer，需要解压再处理
            int protocolVersion = BitConverter.ToInt32(new byte[4] { data[7], data[6], 0, 0 }, 0);
            //操作类型。3=心跳回应，内容为房间人气值；5=通知，弹幕、广播等全部信息；8=进房回应，空
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

                if (protocolVersion == 2)
                {
                    body = DecompressData(body);

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
                                Color = color==0?Color.White: Utils.NumberToColor(color),
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
        private byte[] DecompressData(byte[] data)
        {
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
    }
}
