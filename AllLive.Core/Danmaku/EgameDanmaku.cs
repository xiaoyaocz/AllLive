using AllLive.Core.Interface;
using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.Core.Danmaku
{
    public class EgameDanmaku : ILiveDanmaku
    {
        public int HeartbeatTime => 60;

        public event EventHandler<LiveMessage> NewMessage;
        public event EventHandler<string> OnClose;

        public void Heartbeat()
        {
            throw new NotImplementedException();
        }

        public Task Start(object args)
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
