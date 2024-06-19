using AllLive.Core.Helper;
using AllLive.UWP.Helper;
using Newtonsoft.Json.Linq;
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
using System.Timers;
using AllLive.Core.Danmaku.Proto;
using NLog.Fluent;
using System.Xml.Linq;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace AllLive.UWP.Controls
{
    public sealed partial class BiliLoginDialog : ContentDialog
    {
        Timer timer;
        public BiliLoginDialog()
        {
            this.InitializeComponent();
            this.Loaded += BiliLoginDialog_Loaded;
            this.Unloaded += BiliLoginDialog_Unloaded; ;
        }

        private void BiliLoginDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            timer?.Close();
        }

        private void BiliLoginDialog_Loaded(object sender, RoutedEventArgs e)
        {
            LoadQRCode();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            LoadQRCode();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void imgQR_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LoadQRCode();
        }

        string qrcodeUrl = "";
        string qrcodeKey = "";
        private async void LoadQRCode()
        {
            try
            {
                loaddingImage.Visibility = Visibility.Visible;
                txtStatus.Text = "正在获取二维码...";
                imgQR.Source = null;
                var qrResp = await HttpUtil.GetString("https://passport.bilibili.com/x/passport-login/web/qrcode/generate");
                var json = JObject.Parse(qrResp);
                if (json["code"].ToString() == "0")
                {

                    txtStatus.Text = "等待扫描";
                    qrcodeKey = json["data"]["qrcode_key"].ToString();
                    qrcodeUrl = json["data"]["url"].ToString();

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
                    var qrCodeImage = qrCode.Write(qrcodeUrl);
                    imgQR.Source = qrCodeImage;
                    StartPoll();
                }
                else
                {
                    txtStatus.Text = json["message"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("加载哔哩哔哩登录二维码失败", LogType.ERROR, ex);
                txtStatus.Text = "二维码加载失败";
            }
            finally
            {
                loaddingImage.Visibility = Visibility.Collapsed;
            }
        }
        private async void PollQRStatus()
        {
            try
            {

                var response = await HttpUtil.Get($"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcodeKey}");

                var respContent = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(respContent);
                if (json["code"].ToString() != "0")
                {
                    return;
                }

                var data = json["data"];
                var code = data["code"].ToInt32();
                if (code == 0)
                {
                    var cookies = new List<string>();
                    long userId = 0;
                    foreach (var item in response.Headers.GetValues("Set-Cookie"))
                    {
                        var cookie = item.Split(';')[0];
                        if (cookie.Contains("DedeUserID"))
                        {
                            long.TryParse(cookie.Split('=')[1], out userId);
                        }
                        cookies.Add(cookie);
                    }


                    if (cookies.Count > 0)
                    {
                        var cookieStr = cookies.Aggregate((x, y) => x + ";" + y);
                        SettingHelper.SetValue(SettingHelper.BILI_COOKIE, cookieStr);
                        SettingHelper.SetValue(SettingHelper.BILI_USER_ID, userId);
                        await BiliAccount.Instance.LoadUserInfo();

                        this.Hide();
                    }
                }
                else if (code == 86038)
                {
                    txtStatus.Text = "二维码已过期";
                    qrcodeKey = "";
                    timer?.Close();
                }
                else if (code == 86090)
                {
                    txtStatus.Text = "已扫描，请确认登录";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("轮询二维码失败", LogType.ERROR, ex);
                txtStatus.Text = "轮询失败";
            }

        }
        private void StartPoll()
        {
            timer?.Close();
            timer = new Timer(3 * 1000);
            timer.Elapsed += (sender, e) =>
            {
                _ = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    PollQRStatus();
                });
            };
            timer.Start();
        }



    }
}
