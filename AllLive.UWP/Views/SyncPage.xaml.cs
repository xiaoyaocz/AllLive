
using AllLive.UWP.Helper;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AllLive.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SyncPage : Page
    {
        readonly SyncVM syncVM;
        public SyncPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            syncVM = new SyncVM();
            syncVM.Dispatcher = this.Dispatcher;
        }

        private void btnCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            syncVM.ConnectSignalR("");
        }

        private async void btnJoinRoom_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog= new ContentDialog();
            dialog.Title = "加入房间";
            TextBox textBox = new TextBox();
            textBox.PlaceholderText = "请输入房间号";
            dialog.Content = textBox;
            dialog.PrimaryButtonText = "加入";
            dialog.SecondaryButtonText = "取消";
            dialog.PrimaryButtonClick +=  (s, a) =>
            {
                a.Cancel = true;
                if(string.IsNullOrEmpty(textBox.Text)||textBox.Text.Length!=5)
                {
                    Utils.ShowMessageToast("请输入5位房间号");
                    return;
                }
                dialog.Hide();
                syncVM.ConnectSignalR(textBox.Text);
            };
            await dialog.ShowAsync();
        }

        private void btnQR_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "二维码",
                IsSecondaryButtonEnabled=false,
                PrimaryButtonText = "关闭"
            };
          
            Image image = new Image() { 
                Width=260,
                Height=260
            };
            // 创建二维码
            var qrCode = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 260,
                    Height = 260,
                    Margin = 4,
                }
            };
            var qrCodeImage = qrCode.Write(syncVM.RoomID);
            image.Source = qrCodeImage;
            dialog.Content = image;
            dialog.PrimaryButtonClick += (s, a) =>
            {
                dialog.Hide();
            };
            _ = dialog.ShowAsync();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            syncVM.DisconnectSignalR();
        }

        private void btnSendFollow_Click(object sender, RoutedEventArgs e)
        {
            syncVM.SendFollow();
        }

        private void btnSendHistory_Click(object sender, RoutedEventArgs e)
        {
            syncVM.SendHistory();
        }

        private void btnSendShieldWord_Click(object sender, RoutedEventArgs e)
        {
            syncVM.SendShieldWord();
        }

        private void btnSendBiliBili_Click(object sender, RoutedEventArgs e)
        {
            syncVM.SendBiliBili();
        }
    }
}
