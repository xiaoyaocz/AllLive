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
using System.ComponentModel;
using System.Timers;

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
            SuperChatMessages = new ObservableCollection<SuperChatItem>();
            MessageCleanCount = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, 200);
            KeepSC = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, true);
            AddFavoriteCommand = new RelayCommand(AddFavorite);
            RemoveFavoriteCommand = new RelayCommand(RemoveFavorite);
            
        }
        public ICommand AddFavoriteCommand { get; set; }
        public ICommand RemoveFavoriteCommand { get; set; }
        public int MessageCleanCount { get; set; } = 200;

        /// <summary>
        /// 保留SC
        /// </summary>
        private bool _keepSC;

        public bool KeepSC
        {
            get { return _keepSC; }
            set { _keepSC = value; DoPropertyChanged("KeepSC"); }
        }



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

        private bool _isFavorite = false;
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
                ChangedPlayUrl?.Invoke(this, value.Url);
            }

        }

        public ObservableCollection<LiveMessage> Messages { get; set; }
        public ObservableCollection<SuperChatItem> SuperChatMessages { get; set; }

        public List<SettingsItem<double>> DanmakuOpacityItems { get; } = new List<SettingsItem<double>>()
        {
            new SettingsItem<double>(){ Name="100%",Value=1},
            new SettingsItem<double>(){ Name="90%",Value=0.9},
            new SettingsItem<double>(){ Name="80%",Value=0.8},
            new SettingsItem<double>(){ Name="70%",Value=0.7},
            new SettingsItem<double>(){ Name="60%",Value=0.6},
            new SettingsItem<double>(){ Name="50%",Value=0.5},
            new SettingsItem<double>(){ Name="40%",Value=0.4},
            new SettingsItem<double>(){ Name="30%",Value=0.3},
            new SettingsItem<double>(){ Name="20%",Value=0.2},
            new SettingsItem<double>(){ Name="10%",Value=0.1},
        };
        public List<SettingsItem<double>> DanmakuDiaplayAreaItems { get; } = new List<SettingsItem<double>>()
        {
            new SettingsItem<double>(){ Name="100%",Value=1},
            new SettingsItem<double>(){ Name="75%",Value=0.75},
            new SettingsItem<double>(){ Name="50%",Value=0.5},
            new SettingsItem<double>(){ Name="25%",Value=0.25},
        };
        public List<SettingsItem<int>> DanmakuSpeedItems { get; } = new List<SettingsItem<int>>()
        {
            new SettingsItem<int>(){ Name="极快",Value=2},
            new SettingsItem<int>(){ Name="很快",Value=4},
            new SettingsItem<int>(){ Name="较快",Value=6},
            new SettingsItem<int>(){ Name="快",Value=8},
            new SettingsItem<int>(){ Name="正常",Value=10},
            new SettingsItem<int>(){ Name="慢",Value=12},
            new SettingsItem<int>(){ Name="较慢",Value=14},
            new SettingsItem<int>(){ Name="很慢",Value=16},
            new SettingsItem<int>(){ Name="极慢",Value=18},
        };
        public List<SettingsItem<double>> DnamakuFontZoomItems { get; } = new List<SettingsItem<double>>()
        {
            new SettingsItem<double>(){ Name="极小",Value=0.2},
            new SettingsItem<double>(){ Name="很小",Value=0.6},
            new SettingsItem<double>(){ Name="较小",Value=0.8},
            new SettingsItem<double>(){ Name="小",Value=0.9},
            new SettingsItem<double>(){ Name="正常",Value=1.0},
            new SettingsItem<double>(){ Name="大",Value=1.1},
            new SettingsItem<double>(){ Name="较大",Value=1.2},
            new SettingsItem<double>(){ Name="很大",Value=1.4},
            new SettingsItem<double>(){ Name="极大",Value=1.8},
            new SettingsItem<double>(){ Name="特大",Value=2.0},
        };


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
                MessageCenter.ChangeTitle(Title + " - " + Name, Site);
                if (!string.IsNullOrEmpty(result.UserAvatar))
                {
                    Photo = result.UserAvatar;
                }
                Living = result.Status;
                //加载SC
                LoadSuperChat();
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
                    Qualities = qualities;
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

        Timer scTimer;
        public void SetSCTimer()
        {
            KeepSC = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, true);
            if (KeepSC)
            {
               _= Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var item in SuperChatMessages)
                    {
                        item.ShowCountdown = false;
                    }
                });
                scTimer?.Stop();
                scTimer?.Dispose();
                scTimer = null;
            }
            else
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var item in SuperChatMessages)
                    {
                        item.ShowCountdown = true;
                        item.CountdownTime = Convert.ToInt32(item.EndTime.Subtract(DateTime.Now).TotalSeconds);
                    }
                });
                scTimer = new Timer(1000);
                scTimer.Elapsed += (s, e) =>
                {
                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        for (var i = 0; i < SuperChatMessages.Count; i++)
                        {
                            var item = SuperChatMessages[i];
                            item.CountdownTime--;
                            if (item.CountdownTime <= 0)
                            {
                                SuperChatMessages.RemoveAt(i);
                            }
                        }
                    });
                    
                };
                scTimer.Start();
            }
        }


        private void AddFavorite()
        {
            if (Site == null || RoomID == null || RoomID == "0" || RoomID == "") return;
            DatabaseHelper.AddFavorite(new Models.FavoriteItem()
            {
                Photo = Photo,
                RoomID = RoomID,
                SiteName = Site.Name,
                UserName = Name
            });
            IsFavorite = true;
            MessageCenter.UpdateFavorite();
        }
        private void RemoveFavorite()
        {
            if (FavoriteID == null)
            {
                return;
            }
            DatabaseHelper.DeleteFavorite(FavoriteID.Value);
            IsFavorite = false;
            MessageCenter.UpdateFavorite();
        }

        public async void LoadPlayUrl()
        {
            try
            {
                var data = await Site.GetPlayUrls(detail, CurrentQuality);
                if (data.Count == 0)
                {
                    Utils.ShowMessageToast("加载播放地址失败");
                    return;
                }
                List<PlayurlLine> ls = new List<PlayurlLine>();
                for (int i = 0; i < data.Count; i++)
                {
                    ls.Add(new PlayurlLine()
                    {
                        Name = $"线路{i + 1}",
                        Url = data[i]
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

        public async void LoadSuperChat()
        {
            try
            {
                var data = await Site.GetSuperChatMessages(RoomID);
                if (data.Count>0)
                {
                    foreach (var item in data)
                    {
                        SuperChatMessages.Insert(0, new SuperChatItem(item, KeepSC ? false : true));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("加载SC失败", LogType.ERROR, ex);
                Utils.ShowMessageToast("加载SC失败");
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
                if (e.Type == LiveMessageType.SuperChat)
                {
                    SuperChatMessages.Insert(0, new SuperChatItem(e.Data as LiveSuperChatMessage, KeepSC ? false : true));
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
                    if (!Utils.IsXbox)
                    {
                        Messages.Add(e);
                    }

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
    public class SettingsItem<T>
    {

        public string Name { get; set; }
        public T Value { get; set; }
    }

    public class SuperChatItem : LiveSuperChatMessage, INotifyPropertyChanged
    {
        public SuperChatItem(LiveSuperChatMessage message,bool showCountdown)
        {
            UserName = message.UserName;
            Face = message.Face;
            Message = message.Message;
            Price = message.Price;
            StartTime = message.StartTime;
            EndTime = message.EndTime;
            BackgroundColor = message.BackgroundColor;
            BackgroundBottomColor = message.BackgroundBottomColor;
            CountdownTime=Convert.ToInt32(EndTime.Subtract(DateTime.Now).TotalSeconds);
            ShowCountdown = showCountdown;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private int _countdownTime = 0;
        public int CountdownTime
        {
            get { return _countdownTime; }
            set { _countdownTime = value; DoPropertyChanged("CountdownTime"); }
        }

        public string StartTimeStr
        {
            get
            {
                return StartTime.ToString("HH:mm:ss");
            }
        }

        private bool showCountdown=false;

        public bool ShowCountdown
        {
            get { return showCountdown; }
            set { showCountdown=value; DoPropertyChanged("ShowCountdown"); }
        }


    }
}
