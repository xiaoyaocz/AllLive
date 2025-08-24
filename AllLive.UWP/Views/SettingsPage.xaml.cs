using AllLive.UWP.Helper;
using AllLive.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class SettingsPage : Page
    {
        readonly SettingVM settingVM;
        public SettingsPage()
        {
            settingVM = new SettingVM();
            this.InitializeComponent();
            if (Utils.IsXbox)
            {
                SettingsPaneDiaplsyMode.Visibility = Visibility.Collapsed;
                SettingsMouseClosePage.Visibility = Visibility.Collapsed;
                SettingsFontSize.Visibility = Visibility.Collapsed;
                SettingsAutoClean.Visibility = Visibility.Collapsed;
                SettingsXboxMode.Visibility = Visibility.Visible;
                SettingsNewWindow.Visibility = Visibility.Collapsed;
            }
            BiliAccount.Instance.OnAccountChanged += BiliAccount_OnAccountChanged; 
            LoadUI();

        }

        private void BiliAccount_OnAccountChanged(object sender, EventArgs e)
        {
            if (BiliAccount.Instance.Logined)
            {
                txtBili.Text = $"已登录：{BiliAccount.Instance.UserName}";
                BtnLoginBili.Visibility = Visibility.Collapsed;
                BtnLogoutBili.Visibility = Visibility.Visible;
            }
            else
            {
                txtBili.Text = "登录可享受高清直播";
                BtnLoginBili.Visibility = Visibility.Visible;
                BtnLogoutBili.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadUI()
        {
            //主题
            cbTheme.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.THEME, 0);
            cbTheme.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbTheme.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.THEME, cbTheme.SelectedIndex);
                    Frame rootFrame = Window.Current.Content as Frame;
                    switch (cbTheme.SelectedIndex)
                    {
                        case 1:
                            rootFrame.RequestedTheme = ElementTheme.Light;
                            break;
                        case 2:
                            rootFrame.RequestedTheme = ElementTheme.Dark;
                            break;
                        default:
                            rootFrame.RequestedTheme = ElementTheme.Default;
                            break;
                    }
                    App.SetTitleBar();
                });
            });

            // xbox操作模式
            cbXboxMode.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.XBOX_MODE, 0);
            cbXboxMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbXboxMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.XBOX_MODE, cbXboxMode.SelectedIndex);
                    Utils.ShowMessageToast("重启应用生效");
                });
            });

            //导航栏显示模式
            cbPaneDisplayMode.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.PANE_DISPLAY_MODE, 0);
            cbPaneDisplayMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbPaneDisplayMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.PANE_DISPLAY_MODE, cbPaneDisplayMode.SelectedIndex);
                    MessageCenter.UpdatePanelDisplayMode();
                });
            });

            //鼠标侧键返回
            swMouseClosePage.IsOn = SettingHelper.GetValue<bool>(SettingHelper.MOUSE_BACK, true);
            swMouseClosePage.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swMouseClosePage.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.MOUSE_BACK, swMouseClosePage.IsOn);
                });
            });
            //视频解码
            cbDecoder.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.VIDEO_DECODER, 0);
            cbDecoder.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbDecoder.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.VIDEO_DECODER, cbDecoder.SelectedIndex);
                });
            });

            numFontsize.Value = SettingHelper.GetValue<double>(SettingHelper.MESSAGE_FONTSIZE, 14.0);
            numFontsize.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numFontsize.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.MESSAGE_FONTSIZE, args.NewValue);
                });
            });

            //新窗口打开
            swNewWindow.IsOn = SettingHelper.GetValue<bool>(SettingHelper.NEW_WINDOW_LIVEROOM, false);
            swNewWindow.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swNewWindow.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.NEW_WINDOW_LIVEROOM, swNewWindow.IsOn);
                });
            });
            //默认铺满窗口
            swDefaultFullWindow.IsOn = SettingHelper.GetValue<bool>(SettingHelper.DEFAULT_FULL_WINDOW, false);
            swDefaultFullWindow.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swDefaultFullWindow.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.DEFAULT_FULL_WINDOW, swDefaultFullWindow.IsOn);
                });
            });
            //弹幕开关
            var state = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.SHOW, true);
            DanmuSettingState.IsOn = state;
            DanmuSettingState.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHOW, DanmuSettingState.IsOn);
            });

            // 保留醒目留言
            var keepSC = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, true);
            SettingKeepSC.IsOn = keepSC;
            SettingKeepSC.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, SettingKeepSC.IsOn);
            });

            //弹幕清理
            numCleanCount.Value = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, 200);
            numCleanCount.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numCleanCount.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, Convert.ToInt32(args.NewValue));
                });
            });
            //弹幕关键词
            LiveDanmuSettingListWords.ItemsSource = settingVM.ShieldWords;


            if(BiliAccount.Instance.Logined)
            {
                txtBili.Text = $"已登录：{BiliAccount.Instance.UserName}";
                BtnLoginBili.Visibility = Visibility.Collapsed;
                BtnLogoutBili.Visibility = Visibility.Visible;
            }
           
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            version.Text = $"{SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}";
        }
        private void RemoveLiveDanmuWord_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            settingVM.ShieldWords.Remove(word);
            SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHIELD_WORD, JsonConvert.SerializeObject(settingVM.ShieldWords));
        }

        private void LiveDanmuSettingTxtWord_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(LiveDanmuSettingTxtWord.Text))
            {
                Utils.ShowMessageToast("关键字不能为空");
                return;
            }
            if (!settingVM.ShieldWords.Contains(LiveDanmuSettingTxtWord.Text))
            {
                settingVM.ShieldWords.Add(LiveDanmuSettingTxtWord.Text);
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHIELD_WORD, JsonConvert.SerializeObject(settingVM.ShieldWords));
            }

            LiveDanmuSettingTxtWord.Text = "";
            SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHIELD_WORD, JsonConvert.SerializeObject(settingVM.ShieldWords));
        }

        private async void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/xiaoyaocz/AllLive"));
        }

        private async void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var logFolder = await storageFolder.CreateFolderAsync("log", Windows.Storage.CreationCollisionOption.OpenIfExists);
            await Launcher.LaunchFolderAsync(logFolder);
        }

        private async void BtnLoginBili_Click(object sender, RoutedEventArgs e)
        {
            if (BiliAccount.Instance.Logined)
            {
                Utils.ShowMessageToast("已登录");
                return;
            }
            var result= await MessageCenter.BiliBiliLogin();
            if (result)
            {
                txtBili.Text = $"已登录：{BiliAccount.Instance.UserName}";
                BtnLoginBili.Visibility = Visibility.Collapsed;
                BtnLogoutBili.Visibility = Visibility.Visible;
            }
        }

        private void BtnLogoutBili_Click(object sender, RoutedEventArgs e)
        {
            BiliAccount.Instance.Logout();
            txtBili.Text = "登录可享受高清直播";
            BtnLoginBili.Visibility = Visibility.Visible;
            BtnLogoutBili.Visibility = Visibility.Collapsed;

        }
    }
}
