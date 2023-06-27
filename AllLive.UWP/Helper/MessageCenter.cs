using AllLive.Core.Interface;
using AllLive.Core.Models;
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
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AllLive.UWP.Helper
{
    public static class MessageCenter
    {
        public delegate void NavigatePageHandler(Type page,object data);
        public static event NavigatePageHandler NavigatePageEvent;
        public delegate void ChangeTitleHandler(string title, string logo);
        public static event ChangeTitleHandler ChangeTitleEvent;
        public static event EventHandler<bool> HideTitlebarEvent;
        public static void OpenLiveRoom(ILiveSite liveSite,LiveRoomItem item)
        {
            var arg = new PageArgs()
            {
                Site = liveSite,
                Data = item
            };
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

        public static void ChangeTitle(string title, ILiveSite site=null)
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
    }
    class BlankPage : Page { }
    
}
