using AllLive.Core.Interface;
using AllLive.Core.Models;
using AllLive.UWP.Controls;
using AllLive.UWP.Models;
using AllLive.UWP.ViewModels;
using AllLive.UWP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AllLive.UWP.Helper
{
    public static class MessageCenter
    {
        public delegate void NavigatePageHandler(Type page, object data);
        public static event NavigatePageHandler NavigatePageEvent;
        public delegate void ChangeTitleHandler(string title, string logo);
        public static event ChangeTitleHandler ChangeTitleEvent;
        public static event EventHandler<bool> HideTitlebarEvent;
        public static event EventHandler UpdateFavoriteEvent;
        public static event EventHandler UpdatePanelDisplayModeEvent;
        public async static void OpenLiveRoom(ILiveSite liveSite, LiveRoomItem item)
        {
            var arg = new PageArgs()
            {
                Site = liveSite,
                Data = item
            };

            // 如果是哔哩哔哩
            if (liveSite.Name == "哔哩哔哩直播" && !BiliAccount.Instance.Logined&&!SettingHelper.GetValue(SettingHelper.IGNORE_BILI_LOGIN_TIP,false))
            {
                // 弹窗询问是否登录
                MessageDialog dialog = new MessageDialog("您尚未登录哔哩哔哩账号，部分直播可能无法观看，是否前往登录账号？", "未登录");
                dialog.Commands.Add(new UICommand("登录", async (cmd) =>
                {
                    // 调用登录方法
                    var login = await BiliBiliLogin();
                    if (login)
                    {
                        // 登录成功后打开直播间
                        NavigatePage(typeof(LiveRoomPage), arg);
                    }
                    else
                    {
                        Utils.ShowMessageToast("未登录成功");
                        NavigatePage(typeof(LiveRoomPage), arg);
                    }
                }));
                dialog.Commands.Add(new UICommand("取消", (cmd) =>
                 {
                   NavigatePage(typeof(LiveRoomPage), arg);
                }));
                dialog.Commands.Add(new UICommand("不再提示", (cmd) =>
                {
                    SettingHelper.SetValue(SettingHelper.IGNORE_BILI_LOGIN_TIP, true);
                    NavigatePage(typeof(LiveRoomPage), arg);
                }));
                await dialog.ShowAsync();
                return;
            }

            //if (SettingHelper.GetValue(SettingHelper.NEW_WINDOW_LIVEROOM,false))
            //{
            //    CoreApplicationView newView = CoreApplication.CreateNewView();
            //    int newViewId = 0;
            //    await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //    {
            //        Frame frame = new Frame();
            //        frame.Navigate(typeof(LiveRoomPage), arg);
            //        Window.Current.Content = frame;
            //        Window.Current.Activate();
            //        newViewId = ApplicationView.GetForCurrentView().Id;
            //        ApplicationView.GetForCurrentView().Consolidated += (sender, args) =>
            //        {
            //            frame.Navigate(typeof(BlankPage));
            //            CoreWindow.GetForCurrentThread().Close();
            //        };
            //    });
            //    bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            //}
            //else
            //{
            NavigatePage(typeof(LiveRoomPage), arg);
            //(Window.Current.Content as Frame).Navigate(typeof(LiveRoomPage), arg);
            //}

        }

        public static void NavigatePage(Type page, object data)
        {
            NavigatePageEvent?.Invoke(page, data);
        }

        public static void ChangeTitle(string title, ILiveSite site = null)
        {
            var logo = "ms-appx:///Assets/Square44x44Logo.png";
            if (site != null)
            {
                var siteInfo = MainVM.Sites.FirstOrDefault(x => x.LiveSite.Equals(site));
                if (siteInfo != null)
                {
                    logo = siteInfo.Logo;
                }
            }

            ChangeTitleEvent?.Invoke(title, logo);
        }
        public static void HideTitlebar(bool show)
        {
            HideTitlebarEvent?.Invoke(null, show);
        }

        public static void UpdateFavorite()
        {
            UpdateFavoriteEvent?.Invoke(null, new EventArgs());
        }
        public static void UpdatePanelDisplayMode()
        {
            UpdatePanelDisplayModeEvent?.Invoke(null, new EventArgs());
        }
        public static async Task<bool> BiliBiliLogin()
        {
            BiliLoginDialog biliLoginDialog = new BiliLoginDialog();
            await biliLoginDialog.ShowAsync();
            return BiliAccount.Instance.Logined;

        }
    }
    class BlankPage : Page { }

}
