using AllLive.Core.Interface;
using AllLive.Core.Helper;
using AllLive.UWP.Helper;
using AllLive.UWP.Models;
using AllLive.UWP.ViewModels;
using AllLive.UWP.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AllLive.Core.Models;
using Windows.ApplicationModel.Core;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace AllLive.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.InitializeComponent();
         
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Helper.Utils.CheckVersion();
        }

        private  void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            var item = args.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem;
            if (item.Tag.ToString() == "设置" || item.Tag.ToString() == "Settings")
            {
                item.Tag = "SettingsPage";
            }
            frame.Navigate(Type.GetType("AllLive.UWP.Views." + item.Tag));

        }
        
        private void searchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.QueryText))
            {
                Helper.Utils.ShowMessageToast("关键字不能为空");
                return;
            }
            if (!ParseUrl(args.QueryText))
            {
                this.Frame.Navigate(typeof(SearchPage), args.QueryText);
            }


        }

        private bool ParseUrl(string url)
        {
            ILiveSite site = null;
            var id = "";
            if (url.Contains("bilibili.com"))
            {
                id = url.MatchText(@"bilibili\.com/([\d|\w]+)", "");
                site = MainVM.Sites[0].LiveSite;
            }

            if (url.Contains("douyu.com"))
            {
                id = url.MatchText(@"douyu\.com/([\d|\w]+)", "");
                site = MainVM.Sites[1].LiveSite;
            }
            if (url.Contains("huya.com"))
            {

                id = url.MatchText(@"huya\.com/([\d|\w]+)", "");
                site = MainVM.Sites[2].LiveSite;
            }
            if (site != null && !string.IsNullOrEmpty(id))
            {
                this.Frame.Navigate(typeof(LiveRoomPage), new PageArgs()
                {
                    Site = site,
                    Data = new LiveRoomItem()
                    {
                        RoomID = id
                    }
                });
                return true;
            }
            else
            {
                return false;
            }


        }

    }
}
