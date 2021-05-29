using AllLive.Core.Interface;
using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Core;

namespace AllLive.UWP.ViewModels
{
    public class LiveRoomVM : BaseViewModel
    {
        public LiveRoomVM()
        {
            Messages = new ObservableCollection<LiveMessage>();
        }
        ILiveSite Site;
        ILiveDanmaku LiveDanmaku;

        private string _siteLogo= "ms-appx:///Assets/Placeholder/Placeholder1x1.png";

        public string SiteLogo
        {
            get { return _siteLogo; }
            set { _siteLogo = value; DoPropertyChanged("SiteLogo"); }
        }

        private string _siteName;
        public string SiteName
        {
            get { return _siteName; }
            set { _siteName = value; DoPropertyChanged("SiteName"); }
        }

        object RoomId;
        //private LiveRoomDetail detail;
        //public LiveRoomDetail Detail
        //{
        //    get { return detail; }
        //    set { detail = value; DoPropertyChanged("Detail"); }
        //}
        private long _Online = 0;
        public long Online
        {
            get { return _Online; }
            set { _Online = value; DoPropertyChanged("Online"); }
        }
        private string _RoomID;

        public string RoomID
        {
            get { return _RoomID; }
            set { _RoomID = value; DoPropertyChanged("RoomID"); }
        }


        private string _photo= "ms-appx:///Assets/Placeholder/Placeholder1x1.png";
        public string Photo
        {
            get { return _photo; }
            set { _photo = value; DoPropertyChanged("Photo"); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; DoPropertyChanged("Name"); }
        }
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; DoPropertyChanged("Title"); }
        }
        private bool _living=true;

        public bool Living
        {
            get { return _living; }
            set { _living = value; DoPropertyChanged("Living"); }
        }

        public ObservableCollection<LiveMessage> Messages { get; set; }

        public async void LoadData(ILiveSite site, object roomId)
        {
            try
            {
                Loading = true;
                Site = site;
              
                RoomId = roomId;
                var result = await Site.GetRoomDetail(roomId);
                // detail = result;
                RoomID = result.RoomID;
                 Online = result.Online;
                Title = result.Title;
                Name = result.UserName;
                Photo = result.UserAvatar;
                Living = result.Status;
                LiveDanmaku = Site.GetDanmaku();
                Messages.Add(new LiveMessage()
                {
                    Type = LiveMessageType.Chat,
                    UserName = "系统",
                    Message = "开始接收弹幕"
                });
                LiveDanmaku.NewMessage += LiveDanmaku_NewMessage;
                LiveDanmaku.OnClose += LiveDanmaku_OnClose;
                await LiveDanmaku.Start(result.DanmakuData);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                Loading = false;
            }
        }

        private async void LiveDanmaku_OnClose(object sender, string e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Messages.Add(new LiveMessage()
                {
                    Type = LiveMessageType.Chat,
                    UserName = "系统",
                    Message = "连接已经关闭"
                });
            });
        }

        private async void LiveDanmaku_NewMessage(object sender, LiveMessage e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.Type == LiveMessageType.Online)
                {
                    Online = Convert.ToInt64(e.Data);
                    return;
                }
                if (e.Type == LiveMessageType.Chat)
                {
                    Messages.Add(e);
                    //TODO 检查关键字
                    //TODO 清理互动区
                    return;
                }
            });

        }

        public async void Stop()
        {
            Messages.Clear();
            Messages = null;
            if (LiveDanmaku != null)
            {
                LiveDanmaku.NewMessage -= LiveDanmaku_NewMessage;
                LiveDanmaku.OnClose -= LiveDanmaku_OnClose;
                await LiveDanmaku.Stop();
                LiveDanmaku = null;
            }

        }
    }
}
