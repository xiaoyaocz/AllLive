using AllLive.UWP.Models;
using AllLive.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AllLive.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveRoomPage : Page
    {
        readonly LiveRoomVM liveRoomVM;
        PageArgs pageArgs;
        public LiveRoomPage()
        {
            liveRoomVM = new LiveRoomVM();
            this.InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode== NavigationMode.New)
            {
                pageArgs = e.Parameter as PageArgs;
                var data = pageArgs.Data as LiveRoomItem;
                liveRoomVM.LoadData(pageArgs.Site,data.RoomID);

            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            liveRoomVM.Stop();
            base.OnNavigatedFrom(e);
        }
    }
}
