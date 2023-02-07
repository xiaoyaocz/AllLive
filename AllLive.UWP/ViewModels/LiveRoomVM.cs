using AllLive.Core.Interface;
using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using AllLive.UWP.Helper;
using System.Windows.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;

namespace AllLive.UWP.ViewModels
{
    public class LiveRoomVM : BaseViewModel
    {
        SettingVM settingVM;
        public event EventHandler<string> ChangedPlayUrl;
        public event EventHandler<LiveMessage> AddDanmaku;
        public LiveRoomVM(SettingVM settingVM)
        {
            this.settingVM = settingVM;
            Messages = new ObservableCollection<LiveMessage>();
            MessageCleanCount = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, 200);

            AddFavoriteCommand = new RelayCommand(AddFavorite);
            RemoveFavoriteCommand = new RelayCommand(RemoveFavorite);
        }
        public ICommand AddFavoriteCommand { get; set; }
        public ICommand RemoveFavoriteCommand { get; set; }
        public int MessageCleanCount { get; set; } = 200;

        ILiveSite Site;
        ILiveDanmaku LiveDanmaku;

        private string _siteLogo = "ms-appx:///Assets/Placeholder/Placeholder1x1.png";

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

        private bool _isFavorite=false;
        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { _isFavorite = value; DoPropertyChanged("IsFavorite"); }
        }

        private long? FavoriteID { get; set; }

        object RoomId;
        public LiveRoomDetail detail { get; set; }

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


        private string _photo = "ms-appx:///Assets/Placeholder/Placeholder1x1.png";
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
        private bool _living = true;

        public bool Living
        {
            get { return _living; }
            set { _living = value; DoPropertyChanged("Living"); }
        }


        private List<LivePlayQuality> qualities;
        public List<LivePlayQuality> Qualities
        {
            get { return qualities; }
            set { qualities = value; DoPropertyChanged("Qualities"); }
        }

        private LivePlayQuality currentQuality;
        public LivePlayQuality CurrentQuality
        {
            get { return currentQuality; }
            set
            {
                if (value == null || value == currentQuality)
                {
                    return;
                }
                currentQuality = value;
                DoPropertyChanged("CurrentQuality");
                LoadPlayUrl();
            }

        }

        private List<PlayurlLine> lines;
        public List<PlayurlLine> Lines
        {
            get { return lines; }
            set { lines = value; DoPropertyChanged("Lines"); }
        }

        private PlayurlLine currentLine;
        public PlayurlLine CurrentLine
        {
            get { return currentLine; }
            set
            {
                if (value == null || value == currentLine)
                {
                    return;
                }
                currentLine = value;
                DoPropertyChanged("CurrentLine");
                ChangedPlayUrl?.Invoke(this,value.Url);
            }

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
                detail = result;
                RoomID = result.RoomID;
             
                Online = result.Online;
                Title = result.Title;
                Name = result.UserName;
                MessageCenter.ChangeTitle(Title + " - "+ Name, Site);
                if (!string.IsNullOrEmpty(result.UserAvatar))
                {
                    Photo = result.UserAvatar;
                }
                Living = result.Status;

                //检查收藏情况
                FavoriteID = DatabaseHelper.CheckFavorite(RoomID, Site.Name);
                IsFavorite = FavoriteID != null;
           
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
                if (detail.Status)
                {
                    var qualities = await Site.GetPlayQuality(result);
                    if (Site.Name == "虎牙直播")
                    {
                        //HDR无法播放
                        qualities = qualities.Where(x => !x.Quality.Contains("HDR")).ToList();
                    }
                    Qualities=qualities;
                    if (Qualities != null && Qualities.Count > 0)
                    {
                        CurrentQuality = Qualities[0];
                    }
                    // var u = await Site.GetPlayUrls(result, q[0]);
                    //ChangedPlayUrl?.Invoke(this, u[0]);
                }
                DatabaseHelper.AddHistory(new Models.HistoryItem()
                {
                    Photo = Photo,
                    RoomID = RoomID,
                    SiteName = Site.Name,
                    UserName = Name
                });
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

        private void AddFavorite()
        {
            if (Site == null || RoomID == null) return;
            DatabaseHelper.AddFavorite(new Models.FavoriteItem() { 
                Photo= Photo,
                RoomID=RoomID,
                SiteName=Site.Name,
                UserName= Name
            });
            IsFavorite = true;
        }
        private void RemoveFavorite()
        {
            if (FavoriteID==null)
            {
                return;
            }
            DatabaseHelper.DeleteFavorite(FavoriteID.Value);
            IsFavorite = false;
        }

        public async void LoadPlayUrl()
        {
            try
            {
               var  data = await Site.GetPlayUrls(detail,CurrentQuality);
                if (data.Count == 0)
                {
                    Utils.ShowMessageToast("加载播放地址失败");
                    return;
                }
                List<PlayurlLine> ls = new List<PlayurlLine>();
                for (int i = 0; i < data.Count; i++)
                {
                    ls.Add(new PlayurlLine() { 
                        Name=$"线路{i+1}",
                        Url= data[i]
                    });
                }
              
                Lines = ls;
                CurrentLine = Lines[0];
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("加载播放地址失败");
            }
            



        }


        private async void LiveDanmaku_OnClose(object sender, string e)
        {
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Messages.Add(new LiveMessage()
                {
                    Type = LiveMessageType.Chat,
                    UserName = "系统",
                    Message = "连接已经关闭"
                });
            });
        }
        public CoreDispatcher Dispatcher { get; set; }
        private async void LiveDanmaku_NewMessage(object sender, LiveMessage e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.Type == LiveMessageType.Online)
                {
                    Online = Convert.ToInt64(e.Data);
                    return;
                }
                if (e.Type == LiveMessageType.Chat)
                {
                    if (Messages.Count >= MessageCleanCount)
                    {
                        Messages.RemoveAt(0);
                        //Messages.Clear();
                    }
                    if (settingVM.ShieldWords != null && settingVM.ShieldWords.Count > 0)
                    {
                        if (settingVM.ShieldWords.FirstOrDefault(x => e.Message.Contains(x)) != null) return;
                    }

                    Messages.Add(e);
                    AddDanmaku?.Invoke(this, e);
                    return;
                }
            });
        }

        public async void Stop()
        {
            Messages.Clear();
            if (LiveDanmaku != null)
            {
                LiveDanmaku.NewMessage -= LiveDanmaku_NewMessage;
                LiveDanmaku.OnClose -= LiveDanmaku_OnClose;
                await LiveDanmaku.Stop();
                LiveDanmaku = null;
            }

        }
    }

    public class PlayurlLine
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
