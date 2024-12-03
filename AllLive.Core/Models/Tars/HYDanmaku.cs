using System;
using System.Collections.Generic;
using System.Text;
using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
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
    public class HYPushMessageV2 : TarsStruct
    {


        public string GroupId = "";
        public HYMsgItem[] MsgItem = new HYMsgItem[] { };
        public int ProtocolType = 0;
        public override void ReadFrom(TarsInputStream _is)
        {
            GroupId = _is.Read(GroupId, 0, false);
            MsgItem = _is.readArray<HYMsgItem>(MsgItem, 1, false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(GroupId, 0);
            _os.Write(MsgItem, 1);
        }
    }
    public class HYMsgItem : TarsStruct
    {
        public long Uri = 0;
        public byte[] Msg = new byte[0];
        public long MsgId = 0;
        public override void ReadFrom(TarsInputStream _is)
        {
            Uri = _is.Read(Uri, 0, false);
            Msg = _is.Read(Msg, 1, false);
            MsgId = _is.Read(MsgId, 2, false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(Uri, 0);
            _os.Write(Msg, 1);
            _os.Write(MsgId, 2);
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
        public int FontColor = 0;
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
