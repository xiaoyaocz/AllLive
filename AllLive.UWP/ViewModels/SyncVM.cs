using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR.Client;
using AllLive.UWP.Helper;
using System.Data.Common;
using System.Data;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using AllLive.Core.Danmaku.Proto;
using Windows.UI.Xaml;
using Windows.UI.Core;
using System.Timers;
using Windows.UI.Popups;
using AllLive.UWP.Models;
using Windows.System;
using Newtonsoft.Json.Linq;

namespace AllLive.UWP.ViewModels
{
    public class SyncVM : BaseViewModel
    {
        const string URL = "https://sync1.nsapps.cn/sync";
        public SyncVM() { }

        private bool _RoomConnected = false;
        public bool RoomConnected
        {
            get { return _RoomConnected; }
            set { _RoomConnected = value; DoPropertyChanged("RoomConnected"); }
        }

        private string _RoomID = "--";
        public string RoomID
        {
            get { return _RoomID; }
            set { _RoomID = value; DoPropertyChanged("RoomID"); }
        }

        private bool _IsCreator = false;
        public bool IsCreator
        {
            get { return _IsCreator; }
            set { _IsCreator = value; DoPropertyChanged("IsCreator"); }
        }

        public ObservableCollection<RoomUser> RoomUsers { get; set; } = new ObservableCollection<RoomUser>();

        private bool _SignalRConnecting = false;
        public bool SignalRConnecting
        {
            get { return _SignalRConnecting; }
            set { _SignalRConnecting = value; DoPropertyChanged("SignalRConnecting"); }
        }

        private int _Countdown = 600;
        public int Countdown
        {
            get { return _Countdown; }
            set { _Countdown = value; DoPropertyChanged("Countdown"); }
        }

        HubConnection connection;
        public async void ConnectSignalR(string roomId)
        {
            try
            {
                if (SignalRConnecting)
                {
                    Utils.ShowMessageToast("正在连接中");
                    return;
                }
                SignalRConnecting = true;
                connection = new HubConnectionBuilder()
                   .WithUrl(URL)
                   .Build();
                connection.Closed += async (error) =>
                {
                    RoomConnected = false;
                    Utils.ShowMessageToast("连接已断开");
                    LogHelper.Log("连接已断开", LogType.ERROR, error);
                    await Task.CompletedTask;
                };
                await connection.StartAsync();
                ListenSignalR();
                if (roomId == null || roomId == "")
                {
                    IsCreator = true;
                    await CreateRoom();
                }
                else
                {
                    IsCreator = false;
                    await JoinRoom(roomId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("连接失败", LogType.ERROR, ex);
                Utils.ShowMessageToast($"连接失败：{ex.Message}");
            }
            finally
            {
                SignalRConnecting = false;
            }
        }

        public void ListenSignalR()
        {
            connection.On<bool, string>("onFavoriteReceived", (overlay, content) =>
            {
                ReceiveFavorite(overlay, content);
            });
            connection.On<bool, string>("onHistoryReceived", (overlay, content) =>
            {
                ReceiveHistory(overlay, content);
            });
            connection.On<bool, string>("onShieldWordReceived", (overlay, content) =>
            {
                ReceiveShieldWord(overlay, content);
            });
            connection.On<bool, string>("onBiliAccountReceived", (overlay, content) =>
            {
                ReceiveBiliBili(overlay, content);
            });
            connection.On<string>("onRoomDestroyed", (roomName) =>
            {
                ShowMessage("房间已销毁");
                DisconnectSignalR();
            });

            connection.On<List<RoomUser>>("onUserUpdated", (user) =>
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    RoomUsers.Clear();
                    foreach (var u in user)
                    {
                        if (u.ConnectionId == connection.ConnectionId)
                        {
                            u.IsCurrentUser = true;
                        }
                        RoomUsers.Add(u);
                    }
                });
            });

        }

        private void ReceiveFavorite(bool overlay, string content)
        {
            if (overlay)
            {
                DatabaseHelper.DeleteFavorite();
            }
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FavoriteJsonItem>>(content);
            foreach (var item in items)
            {
                if (DatabaseHelper.CheckFavorite(item.RoomId, item.SiteName) == null)
                {
                    DatabaseHelper.AddFavorite(new FavoriteItem()
                    {
                        SiteName = item.SiteName,
                        RoomID = item.RoomId,
                        UserName = item.UserName,
                        Photo = item.Face,
                    });
                }
            }
            ShowMessage("已同步关注列表");
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 MessageCenter.UpdateFavorite();
             });
        }

        private void ReceiveHistory(bool overlay, string content)
        {
            if (overlay)
            {
                DatabaseHelper.DeleteHistory();
            }
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HistoryJsonItem>>(content);
            foreach (var item in items)
            {
                DatabaseHelper.AddHistory(new HistoryItem()
                {
                    WatchTime = DateTime.Parse(item.UpdateTime),
                    SiteName = item.SiteName,
                    RoomID = item.RoomId,
                    UserName = item.UserName,
                    Photo = item.Face,
                });
            }
            ShowMessage("已同步历史记录");
        }

        private void ReceiveShieldWord(bool overlay, string content)
        {
            var currentWords = JsonConvert.DeserializeObject<List<string>>(SettingHelper.GetValue<string>(SettingHelper.LiveDanmaku.SHIELD_WORD, "[]"));
            if(overlay)
            {
                currentWords.Clear();
            }
            var words = JsonConvert.DeserializeObject<List<string>>(content);
            foreach (var word in words)
            {
                if (!currentWords.Contains(word))
                {
                    currentWords.Add(word);
                }
            }
            SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHIELD_WORD, JsonConvert.SerializeObject(currentWords));
            ShowMessage("已同步屏蔽词");

        }

        private  void ReceiveBiliBili(bool overlay, string content)
        {
            var obj = JObject.Parse(content);
            var cookie = obj["cookie"];
            SettingHelper.SetValue(SettingHelper.BILI_COOKIE, cookie);
            _=Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await BiliAccount.Instance.LoadUserInfo();
            });
          
            ShowMessage("已同步哔哩哔哩账号");
        }

        public CoreDispatcher Dispatcher { get; set; }
        public void ShowMessage(string message)
        {
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Utils.ShowMessageToast(message);
            });
        }

        public void DisconnectSignalR()
        {
            connection?.DisposeAsync();
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SignalRConnecting = false;
                RoomConnected = false;
                RoomID = "--";
                RoomUsers.Clear();
            });
        }

        public async Task CreateRoom()
        {
            if (connection.State != HubConnectionState.Connected)
            {
                Utils.ShowMessageToast("连接已断开");
                return;
            }
            string app = "聚合直播";
            string platform = Utils.IsXbox ? "xbox" : "windows";
            string version = $"{SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}"; ;
            var resp = await connection?.InvokeAsync<Resp<string>>("CreateRoom", app, platform, version);
            if (resp.IsSuccess)
            {
                RoomConnected = true;
                RoomID = resp.Data;
                StartTimer();
            }
            else
            {
                Utils.ShowMessageToast(resp.Message);
                DisconnectSignalR();
            }
        }
        Timer timer;

        private void StartTimer()
        {
            timer?.Stop();
            Countdown = 600;
            timer = new Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Countdown--;
                });
                if (Countdown <= 1)
                {
                    DisconnectSignalR();
                    timer.Stop();
                }
            };
            timer.Start();
        }

        public async Task JoinRoom(string roomId)
        {
            if (connection.State != HubConnectionState.Connected)
            {
                Utils.ShowMessageToast("连接已断开");
                return;
            }
            string app = "聚合直播";
            string platform = Utils.IsXbox ? "xbox" : "windows";
            string version = $"{SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}"; ;
            var resp = await connection?.InvokeAsync<Resp<int>>("JoinRoom", roomId.ToUpper(), app, platform, version);
            if (resp.IsSuccess)
            {
                RoomConnected = true;
                RoomID = roomId.ToUpper();
            }
            else
            {
                Utils.ShowMessageToast(resp.Message);
                DisconnectSignalR();
            }
        }


        public async void SendFollow()
        {
            if (RoomUsers.Count <= 1)
            {
                ShowMessage("无设备连接");
                return;
            }

            var overlay = await ShowOverlayDialog();
            var followList = await DatabaseHelper.GetFavorites();
            if (followList.Count == 0)
            {
                ShowMessage("没有关注的直播间");
                return;
            }

            var items = new List<FavoriteJsonItem>();
            foreach (var item in followList)
            {
                var siteId = "";
                switch (item.SiteName)
                {
                    case "哔哩哔哩直播":
                        siteId = "bilibili";
                        break;
                    case "斗鱼直播":
                        siteId = "douyu";
                        break;
                    case "虎牙直播":
                        siteId = "huya";
                        break;
                    case "抖音直播":
                        siteId = "douyin";
                        break;
                }

                items.Add(new FavoriteJsonItem()
                {
                    SiteId = siteId,
                    Id = $"{siteId}_{item.RoomID}",
                    RoomId = item.RoomID,
                    UserName = item.UserName,
                    Face = item.Photo,
                    AddTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.M")
                });
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(items);
            var resp = await connection?.InvokeAsync<Resp<int>>("SendFavorite", RoomID, overlay, json);
            if (resp.IsSuccess)
            {
                ShowMessage("发送成功");
            }
            else
            {
                ShowMessage(resp.Message);
            }
        }

        public async void SendHistory()
        {
            if (RoomUsers.Count <= 1)
            {
                ShowMessage("无设备连接");
                return;
            }

            var overlay = await ShowOverlayDialog();
            var historyList = await DatabaseHelper.GetHistory();
            if (historyList.Count == 0)
            {
                ShowMessage("暂无历史记录");
                return;
            }
            var items = new List<HistoryJsonItem>();
            foreach (var item in historyList)
            {
                var siteId = "";
                switch (item.SiteName)
                {
                    case "哔哩哔哩直播":
                        siteId = "bilibili";
                        break;
                    case "斗鱼直播":
                        siteId = "douyu";
                        break;
                    case "虎牙直播":
                        siteId = "huya";
                        break;
                    case "抖音直播":
                        siteId = "douyin";
                        break;
                }

                items.Add(new HistoryJsonItem()
                {
                    SiteId = siteId,
                    Id = $"{siteId}_{item.RoomID}",
                    RoomId = item.RoomID,
                    UserName = item.UserName,
                    Face = item.Photo,
                    UpdateTime = item.WatchTime.ToString("yyyy-MM-dd HH:mm:ss.M"),
                });
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(items);
            var resp = await connection?.InvokeAsync<Resp<int>>("SendHistory", RoomID, overlay, json);
            if (resp.IsSuccess)
            {
                ShowMessage("发送成功");
            }
            else
            {
                ShowMessage(resp.Message);
            }
        }

        public async void SendShieldWord()
        {
            if (RoomUsers.Count <= 1)
            {
                ShowMessage("无设备连接");
                return;
            }

            var overlay = await ShowOverlayDialog();
            var currentWords = JsonConvert.DeserializeObject<List<string>>(SettingHelper.GetValue<string>(SettingHelper.LiveDanmaku.SHIELD_WORD, "[]"));
            if (currentWords.Count == 0)
            {
                ShowMessage("暂无屏蔽关键词");
                return;
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(currentWords);
            var resp = await connection?.InvokeAsync<Resp<int>>("SendShieldWord", RoomID, overlay, json);
            if (resp.IsSuccess)
            {
                ShowMessage("发送成功");
            }
            else
            {
                ShowMessage(resp.Message);
            }
        }

        public async void SendBiliBili()
        {
            if (RoomUsers.Count <= 1)
            {
                ShowMessage("无设备连接");
                return;
            }

           
            var cookie = SettingHelper.GetValue<string>(SettingHelper.BILI_COOKIE, "");
            if (cookie == "")
            {
                ShowMessage("未登录哔哩哔哩账号");
                return;
            }
         
            var resp = await connection?.InvokeAsync<Resp<int>>("SendBiliAccount", RoomID, true, JsonConvert.SerializeObject(new { 
                cookie= cookie
            }));
            if (resp.IsSuccess)
            {
                ShowMessage("发送成功");
            }
            else
            {
                ShowMessage(resp.Message);
            }
        }

        private async Task<bool> ShowOverlayDialog()
        {
            var dialog = new MessageDialog("是否覆盖远端数据？", "覆盖数据");
            dialog.Commands.Add(new UICommand("是", null, true));
            dialog.Commands.Add(new UICommand("否", null, false));
            var result = await dialog.ShowAsync();
            return (bool)result.Id;
        }



    }
    public class Resp<T>
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T Data { get; set; }

        public static Resp<T> Error(string message)
        {
            return new Resp<T> { IsSuccess = false, Message = message };
        }

        public static Resp<T> Success(T data)
        {
            return new Resp<T> { IsSuccess = true, Data = data };
        }
    }

    public class RoomUser
    {
        public string ConnectionId { get; set; } = "";
        public string ShortId { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Version { get; set; } = "";
        public string App { get; set; } = "";

        public bool IsCreator { get; set; } = false;

        public bool IsCurrentUser { get; set; } = false;
    }

    public class HistoryJsonItem
    {
        [JsonProperty("siteId")]
        public string SiteId;

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("roomId")]
        public string RoomId;

        [JsonProperty("userName")]
        public string UserName;

        [JsonProperty("face")]
        public string Face;

        [JsonProperty("updateTime")]
        public string UpdateTime;

        [JsonIgnore]
        public string SiteName
        {
            get
            {
                switch (SiteId)
                {
                    case "bilibili":
                        return "哔哩哔哩直播";
                    case "douyu":
                        return "斗鱼直播";
                    case "huya":
                        return "虎牙直播";
                    case "douyin":
                        return "抖音直播";
                    default:
                        return "未知";
                }
            }
        }

    }
}
