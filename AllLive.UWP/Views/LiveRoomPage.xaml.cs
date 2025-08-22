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
using AllLive.UWP.Helper;
using Microsoft.UI.Xaml.Controls;
using NSDanmaku.Model;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using Windows.System.Display;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.ApplicationModel.Core;
using System.Diagnostics;
using Newtonsoft.Json;
using Windows.UI.Core;
using FFmpegInteropX;
using Windows.Media.Playback;
using System.Text;
using System.Runtime;
// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AllLive.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveRoomPage : Page
    {
        readonly LiveRoomVM liveRoomVM;
        readonly SettingVM settingVM;

        FFmpegInteropX.FFmpegMediaSource interopMSS;
        readonly MediaPlayer mediaPlayer;

        DisplayRequest dispRequest;
        PageArgs pageArgs;
        //当前处于小窗
        private bool isMini = false;
        private bool _isNavigatingAway = false; // 新增标志位
        DispatcherTimer timer_focus;
        DispatcherTimer controlTimer;

        public LiveRoomPage()
        {
            this.InitializeComponent();

            settingVM = new SettingVM();
            liveRoomVM = new LiveRoomVM(settingVM);
            liveRoomVM.Dispatcher = this.Dispatcher;
            dispRequest = new DisplayRequest();

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
          
            liveRoomVM.ChangedPlayUrl += LiveRoomVM_ChangedPlayUrl;
            liveRoomVM.AddDanmaku += LiveRoomVM_AddDanmaku;
            //每过2秒就设置焦点
            timer_focus = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            timer_focus.Tick += Timer_focus_Tick;
            controlTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            controlTimer.Tick += ControlTimer_Tick;
            mediaPlayer = new MediaPlayer();
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSession_BufferingStarted;
            mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSession_BufferingProgressChanged;
            mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSession_BufferingEnded;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

            timer_focus.Start();
            controlTimer.Start();
            if (Utils.IsXbox && SettingHelper.GetValue<int>(SettingHelper.XBOX_MODE, 0) == 0)
            {
                XBoxControl.Visibility = Visibility.Visible;
                StandardControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                XBoxControl.Visibility = Visibility.Collapsed;
                ClearXboxSettingBind();
                StandardControl.Visibility = Visibility.Visible;
            }

            // 新窗口打开，调整UI
            if (SettingHelper.GetValue(SettingHelper.NEW_WINDOW_LIVEROOM, false))
            {
                ApplicationView.GetForCurrentView().Consolidated += LiveRoomPage_Consolidated;
                TitleBar.Visibility = Visibility.Visible;
                // 自定义标题栏
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                Window.Current.SetTitleBar(TitleBar);
                SetTitleBarColor();
            }

        }

        private void LiveRoomPage_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            StopPlay();
            // 关闭窗口
            CoreWindow.GetForCurrentThread().Close();
        }

     
        private void SetTitleBarColor()
        {
            var settingTheme = SettingHelper.GetValue<int>(SettingHelper.THEME, 0);
            UISettings uiSettings = new UISettings();
            var color = uiSettings.GetColorValue(UIColorType.Foreground);
            if (settingTheme != 0)
            {
                color = settingTheme == 1 ? Colors.Black : Colors.White;

            }
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = color;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.BackgroundColor = Colors.Transparent;
        }
        private void HideTitleBar(bool hide)
        {
            if (SettingHelper.GetValue(SettingHelper.NEW_WINDOW_LIVEROOM, false))
            {
                if (hide)
                {
                    Grid.SetRow(GridContent, 0);
                    Grid.SetRowSpan(GridContent, 2);
                    TitleBarGrid.Visibility = Visibility.Collapsed;
                    Window.Current.SetTitleBar(null);
                }
                else
                {
                    Grid.SetRow(GridContent, 1);
                    Grid.SetRowSpan(GridContent, 1);
                    TitleBarGrid.Visibility = Visibility.Visible;
                    Window.Current.SetTitleBar(TitleBar);
                }
            }
            else
            {
                MessageCenter.HideTitlebar(hide);
            }
        }
        private void ClearXboxSettingBind()
        {
            XboxSuperChat.ClearValue(ListView.ItemsSourceProperty);
            xboxSettingsDMSize.ClearValue(ComboBox.SelectedValueProperty);
            xboxSettingsDecoder.ClearValue(ToggleSwitch.IsOnProperty);
            xboxSettingsDMArea.ClearValue(ComboBox.SelectedIndexProperty);
            xboxSettingsDMOpacity.ClearValue(ComboBox.SelectedValueProperty);
            xboxSettingsDMSpeed.ClearValue(ComboBox.SelectedValueProperty);
            xboxSettingsDMStyle.ClearValue(ComboBox.SelectedValueProperty);
            xboxSettingsDMColorful.ClearValue(ToggleSwitch.IsOnProperty);
            xboxSettingsDMBold.ClearValue(ToggleSwitch.IsOnProperty);
        }

        #region 播放器事件
        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var index = liveRoomVM.Lines.IndexOf(liveRoomVM.CurrentLine);
                //尝试切换
                if (index == liveRoomVM.Lines.Count - 1)
                {
                    liveRoomVM.Living = false;
                }
                else
                {
                    liveRoomVM.CurrentLine = liveRoomVM.Lines[index + 1];
                }
            });
        }

        private async void PlaybackSession_BufferingEnded(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayerLoading.Visibility = Visibility.Collapsed;
            });

        }

        private async void PlaybackSession_BufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayerLoadText.Text = sender.BufferingProgress.ToString("p");
            });
        }

        private async void PlaybackSession_BufferingStarted(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayerLoading.Visibility = Visibility.Visible;
                PlayerLoadText.Text = "缓冲中";
            });
        }

        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

                PlayError();
            });

        }

        private async void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //保持屏幕常亮
                dispRequest.RequestActive();
                PlayerLoading.Visibility = Visibility.Collapsed;
                SetMediaInfo();
            });
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.PlaybackState)
                {
                    case MediaPlaybackState.None:
                        break;
                    case MediaPlaybackState.Opening:
                        PlayerLoading.Visibility = Visibility.Visible;
                        PlayerLoadText.Text = "加载中";
                        break;
                    case MediaPlaybackState.Buffering:
                        PlayerLoading.Visibility = Visibility.Visible;
                        PlayerLoadText.Text = "缓冲中";
                        break;
                    case MediaPlaybackState.Playing:
                        PlayerLoading.Visibility = Visibility.Collapsed;
                        PlayBtnPlay.Visibility = Visibility.Collapsed;
                        PlayBtnPause.Visibility = Visibility.Visible;
                        dispRequest.RequestActive();
                        liveRoomVM.Living = true;
                        SetMediaInfo();
                        break;
                    case MediaPlaybackState.Paused:
                        PlayerLoading.Visibility = Visibility.Collapsed;
                        PlayBtnPlay.Visibility = Visibility.Visible;
                        PlayBtnPause.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            });
        }

        private void SetMediaInfo()
        {
            try
            {

                var str = $"Url: {liveRoomVM.CurrentLine?.Url ?? ""}\r\n";
                str += $"Quality: {liveRoomVM.CurrentQuality?.Quality ?? ""}\r\n";
                str += $"Video Codec: {interopMSS.CurrentVideoStream.CodecName}\r\nAudio Codec:{interopMSS.AudioStreams[0].CodecName}\r\n";
                str += $"Resolution: {interopMSS.CurrentVideoStream.PixelWidth} x {interopMSS.CurrentVideoStream.PixelHeight}\r\n";
                str += $"Video Bitrate: {interopMSS.CurrentVideoStream.Bitrate / 1024} Kbps\r\n";
                str += $"Audio Bitrate: {interopMSS.AudioStreams[0].Bitrate / 1024} Kbps\r\n";
                str += $"Decoder Engine: {interopMSS.CurrentVideoStream.DecoderEngine.ToString()}";
                txtInfo.Text = str;
            }
            catch (Exception ex)
            {
                txtInfo.Text = $"读取信息失败\r\n{ex.Message}";
            }



        }

        #endregion



        private void LiveRoomVM_AddDanmaku(object sender, LiveMessage e)
        {

            if (DanmuControl.Visibility == Visibility.Visible)
            {
                var color = DanmuSettingColourful.IsOn ?
                    Color.FromArgb(e.Color.A, e.Color.R, e.Color.G, e.Color.B) :
                    Colors.White;
                DanmuControl.AddLiveDanmu(e.Message, false, color);
            }

        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            var elent = FocusManager.GetFocusedElement();
            if (elent is TextBox || elent is AutoSuggestBox)
            {
                args.Handled = false;
                return;
            }
            if (XBoxSplitView.IsPaneOpen)
            {
                if (args.VirtualKey == Windows.System.VirtualKey.GamepadMenu)
                {
                    XBoxSplitView.IsPaneOpen = false;
                    args.Handled = true;
                    return;
                }
                if (args.VirtualKey == Windows.System.VirtualKey.GamepadB)
                {
                    if (XboxSuperChat.Visibility == Visibility.Visible)
                    {
                        XBoxSplitView.IsPaneOpen = false;
                    }
                    args.Handled = true;
                    return;
                }
                args.Handled = false;
                return;
            }
            args.Handled = true;
            switch (args.VirtualKey)
            {
                //case Windows.System.VirtualKey.Space:
                //    if (mediaPlayer.PlaybackSession.CanPause)
                //    {
                //        mediaPlayer.Pause();
                //    }
                //    else
                //    {
                //        mediaPlayer.Play();
                //    }
                //    break;

                case Windows.System.VirtualKey.Up:
                    if (mediaPlayer.Volume + 0.1 > 1)
                    {
                        mediaPlayer.Volume = 1;
                    }
                    else
                    {
                        mediaPlayer.Volume += 0.1;
                    }


                    TxtToolTip.Text = "音量:" + mediaPlayer.Volume.ToString("P");
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;

                case Windows.System.VirtualKey.Down:
                    if (mediaPlayer.Volume - 0.1 < 0)
                    {
                        mediaPlayer.Volume = 0;
                    }
                    else
                    {
                        mediaPlayer.Volume -= 0.1;
                    }


                    if (mediaPlayer.Volume == 0)
                    {
                        TxtToolTip.Text = "静音";
                    }
                    else
                    {
                        TxtToolTip.Text = "音量:" + mediaPlayer.Volume.ToString("P");
                    }
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;
                case Windows.System.VirtualKey.Escape:
                    SetFullScreen(false);

                    break;
                case Windows.System.VirtualKey.F8:
                case Windows.System.VirtualKey.T:
                    //小窗播放
                    MiniWidnows(BottomBtnExitMiniWindows.Visibility == Visibility.Visible);

                    break;
                case Windows.System.VirtualKey.F12:
                case Windows.System.VirtualKey.W:
                    SetFullWindow(PlayBtnFullWindow.Visibility == Visibility.Visible);
                    break;
                case Windows.System.VirtualKey.F11:
                case Windows.System.VirtualKey.F:
                case Windows.System.VirtualKey.Enter:
                    SetFullScreen(PlayBtnFullScreen.Visibility == Visibility.Visible);
                    break;
                case Windows.System.VirtualKey.F10:
                    await CaptureVideo();
                    break;
                case Windows.System.VirtualKey.F9:
                case Windows.System.VirtualKey.D:
                case Windows.System.VirtualKey.GamepadX:
                    //if (DanmuControl.Visibility == Visibility.Visible)
                    //{
                    //    DanmuControl.Visibility = Visibility.Collapsed;

                    //}
                    //else
                    //{
                    //    DanmuControl.Visibility = Visibility.Visible;
                    //}
                    PlaySWDanmu.IsOn = DanmuControl.Visibility != Visibility.Visible;
                    break;
                case Windows.System.VirtualKey.GamepadA:
                    ShowControl(control.Visibility == Visibility.Collapsed);
                    break;
                case Windows.System.VirtualKey.GamepadMenu:
                    //打开设置
                    XBoxSettings.Visibility = Visibility.Visible;
                    XboxSuperChat.Visibility = Visibility.Collapsed;
                    XBoxSplitView.IsPaneOpen = true;
                    break;
                case Windows.System.VirtualKey.GamepadLeftTrigger:
                    //刷新直播间
                    BottomBtnRefresh_Click(this, null);
                    break;
                case Windows.System.VirtualKey.GamepadB:
                    //退出直播间
                    this.Frame.GoBack();
                    break;
                case Windows.System.VirtualKey.GamepadY:
                    //查看SC
                    XBoxSettings.Visibility = Visibility.Collapsed;
                    XboxSuperChat.Visibility = Visibility.Visible;
                    XBoxSplitView.IsPaneOpen = true;
                    break;
                case Windows.System.VirtualKey.GamepadRightTrigger:
                    //关注/取消关注
                    if (liveRoomVM.IsFavorite)
                    {
                        liveRoomVM.RemoveFavoriteCommand.Execute(null);
                        Utils.ShowMessageToast("已取消关注");
                    }
                    else
                    {
                        liveRoomVM.AddFavoriteCommand.Execute(null);
                        Utils.ShowMessageToast("已添加关注");
                    }

                    break;
                default:
                    break;
            }
        }


        private void LiveRoomVM_ChangedPlayUrl(object sender, string e)
        {
            _ = SetPlayer(e);
        }
        private async Task SetPlayer(string url)
        {
            try
            {
                PlayerLoading.Visibility = Visibility.Visible;
                PlayerLoadText.Text = "加载中";
                if (mediaPlayer != null)
                {
                    mediaPlayer.Pause();
                    mediaPlayer.Source = null;
                }
                if (interopMSS != null)
                {
                    interopMSS.Dispose();
                    interopMSS = null;
                }

                var config = new MediaSourceConfig();
                config.FFmpegOptions.Add("rtsp_transport", "tcp");
                var decoder = SettingHelper.GetValue<int>(SettingHelper.VIDEO_DECODER, Utils.IsXbox ? 1 : 0);
                switch (decoder)
                {
                    case 1:
                        config.Video.VideoDecoderMode = VideoDecoderMode.ForceSystemDecoder;
                        break;
                    case 2:
                        config.Video.VideoDecoderMode = VideoDecoderMode.ForceFFmpegSoftwareDecoder;
                        break;
                    default:
                        config.Video.VideoDecoderMode = VideoDecoderMode.Automatic;
                        break;
                }
                if (liveRoomVM.SiteName == "哔哩哔哩直播")
                {
                    config.FFmpegOptions.Add("user_agent", "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)");
                    config.FFmpegOptions.Add("referer", "https://live.bilibili.com/");
                }
                else if (liveRoomVM.SiteName == "虎牙直播")
                {
                    //config.FFmpegOptions.Add("user_agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1");
                    //config.FFmpegOptions.Add("referer", "https://m.huya.com");

                    // from stream-rec url:https://github.com/stream-rec/stream-rec
                    //var sysTs = Utils.GetTimeStamp() / 1000;
                    //var validTs = 20000308;
                    //var last8 = sysTs % 100000000;
                    //var currentTs = last8 > validTs ? last8 : (validTs + sysTs / 100);
                    //config.FFmpegOptions.Add("user_agent", $"HYSDK(Windows, {currentTs})");
                    config.FFmpegOptions.Add("user_agent", "HYSDK(Windows, 30000002)_APP(pc_exe&6080100&official)_SDK(trans&2.23.0.4969)");
                }
                try
                {
                    interopMSS = await FFmpegMediaSource.CreateFromUriAsync(url, config);

                    if (_isNavigatingAway)
                    {
                        interopMSS?.Dispose(); // 清理刚刚创建的资源
                        interopMSS = null;
                        return; // 提前退出，不再设置播放源
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log("播放器初始化失败", LogType.ERROR, ex);
                    PlayError();
                    return;
                }

                mediaPlayer.AutoPlay = true;
                mediaPlayer.Volume = SliderVolume.Value;
                mediaPlayer.Source = interopMSS.CreateMediaPlaybackItem();
                player.SetMediaPlayer(mediaPlayer);
            }
            catch (Exception ex)
            {
                Utils.ShowMessageToast("播放失败" + ex.Message);
            }

        }

        private async void PlayError()
        {
            if (liveRoomVM.CurrentLine == null)
            {
                return;
            }
            // 当前线路播放失败，尝试下一个线路
            var index = liveRoomVM.Lines.IndexOf(liveRoomVM.CurrentLine);
            if (index == liveRoomVM.Lines.Count - 1)
            {
                PlayerLoading.Visibility = Visibility.Collapsed;
                LogHelper.Log("直播加载失败", LogType.ERROR, new Exception("直播加载失败"));
                await new MessageDialog($"啊，播放失败了，请尝试以下操作\r\n1、更换清晰度或线路\r\n2、请尝试在直播设置中打开/关闭硬解试试", "播放失败").ShowAsync();
            }
            else
            {
                liveRoomVM.CurrentLine = liveRoomVM.Lines[index + 1];
            }
        }

        private void StopPlay()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
                mediaPlayer.Source = null;
            }
            if (interopMSS != null)
            {
                interopMSS.Dispose();
                interopMSS = null;
            }

            timer_focus.Stop();
            controlTimer.Stop();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);

            liveRoomVM?.Stop();

            SetFullScreen(false);
            MiniWidnows(false);
            //取消屏幕常亮
            if (dispRequest != null)
            {
                try
                {
                    dispRequest.RequestRelease();
                }
                catch (Exception)
                {
                }

                dispRequest = null;
            }
        }
        private void ControlTimer_Tick(object sender, object e)
        {
            if (showControlsFlag != -1)
            {
                if (showControlsFlag >= 5)
                {
                    var elent = FocusManager.GetFocusedElement();
                    if (!(elent is TextBox) && !(elent is AutoSuggestBox))
                    {
                        ShowControl(false);
                        showControlsFlag = -1;
                    }
                }
                else
                {
                    showControlsFlag++;
                }
            }
        }

        private void Timer_focus_Tick(object sender, object e)
        {
            var elent = FocusManager.GetFocusedElement();
            if (elent is Button || elent is AppBarButton || elent is HyperlinkButton || elent is MenuFlyoutItem)
            {
                BtnFoucs.Focus(FocusState.Programmatic);
            }

        }
        //private void btnBack_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this.Frame.CanGoBack)
        //    {
        //        this.Frame.GoBack();
        //    }
        //}
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _isNavigatingAway = true; // 在所有清理操作之前设置标志位

            liveRoomVM.AddDanmaku -= LiveRoomVM_AddDanmaku;
            StopPlay();

            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;

            base.OnNavigatingFrom(e);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == Windows.UI.Xaml.Navigation.NavigationMode.New)
            {
                pageArgs = e.Parameter as PageArgs;
                if (Utils.IsXbox)
                {
                    LoadSetting();
                    LoadXboxSetting();
                }
                else
                {
                    LoadSetting();
                }

                var siteInfo = MainVM.Sites.FirstOrDefault(x => x.LiveSite.Equals(pageArgs.Site));

                liveRoomVM.SiteLogo = siteInfo.Logo;
                liveRoomVM.SiteName = siteInfo.Name;

                var data = pageArgs.Data as LiveRoomItem;
                MessageCenter.ChangeTitle("", pageArgs.Site);

                liveRoomVM.LoadData(pageArgs.Site, data.RoomID);

                // 如果是XBOX，自动进入全屏
                if (Utils.IsXbox)
                {
                    SetFullScreen(true);
                }
            }
        }

        private async Task CaptureVideo()
        {
            try
            {
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                StorageFolder applicationFolder = KnownFolders.PicturesLibrary;
                StorageFolder folder = await applicationFolder.CreateFolderAsync("直播截图", CreationCollisionOption.OpenIfExists);
                StorageFile saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                RenderTargetBitmap bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(player);
                var pixelBuffer = await bitmap.GetPixelsAsync();
                using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                         (uint)bitmap.PixelWidth,
                         (uint)bitmap.PixelHeight,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                }
                Utils.ShowMessageToast("截图已经保存至图片库");
            }
            catch (Exception)
            {
                Utils.ShowMessageToast("截图失败");
            }
        }



        private void LoadSetting()
        {
            //右侧宽度
            var width = SettingHelper.GetValue<double>(SettingHelper.RIGHT_DETAIL_WIDTH, 280);
            ColumnRight.Width = new GridLength(width, GridUnitType.Pixel);
            GridRight.SizeChanged += new SizeChangedEventHandler((sender, args) =>
            {
                if (args.NewSize.Width <= 0)
                {
                    return;
                }
                SettingHelper.SetValue<double>(SettingHelper.RIGHT_DETAIL_WIDTH, args.NewSize.Width + 16);
            });
            //软解视频
            //cbDecode.SelectedIndex= SettingHelper.GetValue<int>(SettingHelper.DECODE, 0);
            //switch (cbDecode.SelectedIndex)
            //{
            //    case 1:
            //        _config.VideoDecoderMode = VideoDecoderMode.ForceSystemDecoder;
            //        break;
            //    case 2:
            //        _config.VideoDecoderMode = VideoDecoderMode.ForceFFmpegSoftwareDecoder;
            //        break;
            //    default:
            //        _config.VideoDecoderMode = VideoDecoderMode.Automatic;
            //        break;
            //}
            //cbDecode.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.DECODE, 0);
            //cbDecode.Loaded += new RoutedEventHandler((sender, e) =>
            //{
            //    cbDecode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
            //    {
            //        SettingHelper.SetValue(SettingHelper.DECODE, cbDecode.SelectedIndex);
            //        switch (cbDecode.SelectedIndex)
            //        {
            //            case 1:
            //                _config.VideoDecoderMode = VideoDecoderMode.ForceSystemDecoder;
            //                break;
            //            case 2:
            //                _config.VideoDecoderMode = VideoDecoderMode.ForceFFmpegSoftwareDecoder;
            //                break;
            //            default:
            //                _config.VideoDecoderMode = VideoDecoderMode.Automatic;
            //                break;
            //        }
            //        Utils.ShowMessageToast("更改清晰度或刷新后生效");
            //    });
            //});

            //swSoftwareDecode.Loaded += new RoutedEventHandler((sender, e) =>
            //{
            //    swSoftwareDecode.Toggled += new RoutedEventHandler((obj, args) =>
            //    {
            //        SettingHelper.SetValue(SettingHelper.SORTWARE_DECODING, swSoftwareDecode.IsOn);
            //        //if (mediaPlayer != null)
            //        //{
            //        //    mediaPlayer.EnableHardwareDecoding = !swSoftwareDecode.IsOn;
            //        //}

            //        Utils.ShowMessageToast("更改清晰度或刷新后生效");
            //    });
            //});
            cbDecoder.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.VIDEO_DECODER, Utils.IsXbox ? 1 : 0);
            cbDecoder.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbDecoder.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.VIDEO_DECODER, cbDecoder.SelectedIndex);
                    Utils.ShowMessageToast("更改清晰度或刷新后生效");
                });
            });
            //弹幕开关
            var state = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.SHOW, true) ? Visibility.Visible : Visibility.Collapsed;
            DanmuControl.Visibility = state;
            PlaySWDanmu.IsOn = state == Visibility.Visible;
            PlaySWDanmu.Toggled += new RoutedEventHandler((e, args) =>
            {
                var visibility = PlaySWDanmu.IsOn ? Visibility.Visible : Visibility.Collapsed;
                DanmuControl.Visibility = visibility;
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHOW, PlaySWDanmu.IsOn);
            });

            // 保留醒目留言
            var keepSC = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, true);
            swKeepSC.IsOn = keepSC;
            liveRoomVM.SetSCTimer();
            swKeepSC.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, swKeepSC.IsOn);
                liveRoomVM.SetSCTimer();
            });

            //音量
            var volume = SettingHelper.GetValue<double>(SettingHelper.PLAYER_VOLUME, 1.0);
            mediaPlayer.Volume = volume;
            SliderVolume.Value = volume;
            SliderVolume.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                mediaPlayer.Volume = SliderVolume.Value;
                SettingHelper.SetValue<double>(SettingHelper.PLAYER_VOLUME, SliderVolume.Value);
            });
            //亮度
            _brightness = SettingHelper.GetValue<double>(SettingHelper.PLAYER_BRIGHTNESS, 0);
            BrightnessShield.Opacity = _brightness;

            //弹幕清理
            numCleanCount.Value = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, 200);
            numCleanCount.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numCleanCount.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    liveRoomVM.MessageCleanCount = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, Convert.ToInt32(args.NewValue));
                    SettingHelper.SetValue(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, Convert.ToInt32(args.NewValue));
                });
            });

            //互动文字大小
            numFontsize.Value = SettingHelper.GetValue<double>(SettingHelper.MESSAGE_FONTSIZE, 14.0);
            numFontsize.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numFontsize.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.MESSAGE_FONTSIZE, args.NewValue);
                });
            });


            //弹幕关键词
            LiveDanmuSettingListWords.ItemsSource = settingVM.ShieldWords;

            //弹幕顶部距离
            DanmuControl.Margin = new Thickness(0, SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.TOP_MARGIN, 0), 0, 0);
            DanmuTopMargin.Value = DanmuControl.Margin.Top;
            DanmuTopMargin.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.TOP_MARGIN, DanmuTopMargin.Value);
                DanmuControl.Margin = new Thickness(0, DanmuTopMargin.Value, 0, 0);
            });
            //弹幕大小
            DanmuControl.DanmakuSizeZoom = SettingHelper.GetValue<double>(SettingHelper.LiveDanmaku.FONT_ZOOM, 1);
            DanmuSettingFontZoom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (isMini) return;
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.FONT_ZOOM, DanmuSettingFontZoom.Value);
            });
            //弹幕速度
            DanmuControl.DanmakuDuration = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.SPEED, 10);
            DanmuSettingSpeed.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (isMini) return;
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.SPEED, DanmuSettingSpeed.Value);
            });

            //保留一位小数
            DanmuControl.Opacity = SettingHelper.GetValue<double>(SettingHelper.LiveDanmaku.OPACITY, 1.0);
            DanmuSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.OPACITY, DanmuSettingOpacity.Value);
            });
            //弹幕加粗
            DanmuControl.DanmakuBold = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.BOLD, false);
            DanmuSettingBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<bool>(SettingHelper.LiveDanmaku.BOLD, DanmuSettingBold.IsOn);
            });
            //弹幕样式
            var danmuStyle = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.BORDER_STYLE, 2);
            if (danmuStyle > 2)
            {
                danmuStyle = 2;
            }
            DanmuControl.DanmakuStyle = (DanmakuBorderStyle)danmuStyle;
            DanmuSettingStyle.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (DanmuSettingStyle.SelectedIndex != -1)
                {
                    SettingHelper.SetValue<int>(SettingHelper.LiveDanmaku.BORDER_STYLE, DanmuSettingStyle.SelectedIndex);
                }
            });


            //弹幕显示区域
            DanmuControl.DanmakuArea = SettingHelper.GetValue<double>(SettingHelper.LiveDanmaku.AREA, 1);
            DanmuSettingArea.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.AREA, DanmuSettingArea.Value);
            });

            //彩色弹幕
            DanmuSettingColourful.IsOn = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.COLOURFUL, true);
            DanmuSettingColourful.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<bool>(SettingHelper.LiveDanmaku.COLOURFUL, DanmuSettingColourful.IsOn);
            });
        }
        private void LoadXboxSetting()
        {

            xboxSettingsDecoder.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.VIDEO_DECODER, Utils.IsXbox ? 1 : 0);
            xboxSettingsDecoder.Loaded += new RoutedEventHandler((sender, e) =>
            {
                xboxSettingsDecoder.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.VIDEO_DECODER, xboxSettingsDecoder.SelectedIndex);
                    Utils.ShowMessageToast("更改清晰度或刷新后生效");
                });
            });

            //弹幕开关
            var state = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.SHOW, true) ? Visibility.Visible : Visibility.Collapsed;

            PlaySWDanmu.IsOn = state == Visibility.Visible;
            PlaySWDanmu.Toggled += new RoutedEventHandler((e, args) =>
            {
                var visibility = PlaySWDanmu.IsOn ? Visibility.Visible : Visibility.Collapsed;
                DanmuControl.Visibility = visibility;
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHOW, PlaySWDanmu.IsOn);
            });

            ////音量
            var volume = SettingHelper.GetValue<double>(SettingHelper.PLAYER_VOLUME, 1.0);
            SliderVolume.Value = volume;
            SliderVolume.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                mediaPlayer.Volume = SliderVolume.Value;
                SettingHelper.SetValue<double>(SettingHelper.PLAYER_VOLUME, SliderVolume.Value);
            });


            //弹幕关键词
            LiveDanmuSettingListWords.ItemsSource = settingVM.ShieldWords;

            //弹幕大小
            //DanmuControl.DanmakuSizeZoom = SettingHelper.GetValue<double>(SettingHelper.LiveDanmaku.FONT_ZOOM, 1);
            xboxSettingsDMSize.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (xboxSettingsDMSize.SelectedValue == null)
                {
                    return;
                }
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.FONT_ZOOM, (double)xboxSettingsDMSize.SelectedValue);
            });

            //弹幕速度
            //DanmuControl.DanmakuDuration = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.SPEED, 10);
            xboxSettingsDMSpeed.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (xboxSettingsDMSpeed.SelectedValue == null)
                {
                    return;
                }
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.SPEED, (int)xboxSettingsDMSpeed.SelectedValue);
            });

            //弹幕透明度
            //DanmuControl.Opacity = SettingHelper.GetValue<double>(SettingHelper.LiveDanmaku.OPACITY, 1.0);
            xboxSettingsDMOpacity.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (xboxSettingsDMOpacity.SelectedValue == null)
                {
                    return;
                }
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.OPACITY, (double)xboxSettingsDMOpacity.SelectedValue);
            });


            //弹幕加粗
            //DanmuControl.DanmakuBold = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.BOLD, false);
            xboxSettingsDMBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<bool>(SettingHelper.LiveDanmaku.BOLD, xboxSettingsDMBold.IsOn);
            });

            //弹幕样式
            var danmuStyle = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.BORDER_STYLE, 2);
            if (danmuStyle > 2)
            {
                danmuStyle = 2;
            }
            //DanmuControl.DanmakuStyle = (DanmakuBorderStyle)danmuStyle;
            xboxSettingsDMStyle.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (xboxSettingsDMStyle.SelectedIndex != -1)
                {
                    SettingHelper.SetValue<int>(SettingHelper.LiveDanmaku.BORDER_STYLE, xboxSettingsDMStyle.SelectedIndex);
                }
            });


            //弹幕显示区域
            //DanmuControl.DanmakuArea = SettingHelper.GetValue<double>(SettingHelper.LiveDanmaku.AREA, 1);
            xboxSettingsDMArea.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (xboxSettingsDMArea.SelectedValue == null)
                {
                    return;
                }
                SettingHelper.SetValue<double>(SettingHelper.LiveDanmaku.AREA, (double)xboxSettingsDMArea.SelectedValue);
            });

            //彩色弹幕
            xboxSettingsDMColorful.IsOn = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.COLOURFUL, true);
            xboxSettingsDMColorful.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue<bool>(SettingHelper.LiveDanmaku.COLOURFUL, xboxSettingsDMColorful.IsOn);
            });

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


        #region 手势
        int showControlsFlag = 0;
        bool pointer_in_player = false;

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowControl(control.Visibility == Visibility.Collapsed);

        }
        bool runing = false;
        private async void ShowControl(bool show)
        {
            if (runing) return;
            runing = true;
            if (show)
            {
                showControlsFlag = 0;
                control.Visibility = Visibility.Visible;

                await control.FadeInAsync(280);

            }
            else
            {
                if (pointer_in_player)
                {
                    Window.Current.CoreWindow.PointerCursor = null;
                }
                await control.FadeOutAsync(280);
                control.Visibility = Visibility.Collapsed;
            }
            runing = false;
        }
        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (isMini)
            {
                MiniWidnows(false);
                return;
            }

            if (PlayBtnFullScreen.Visibility == Visibility.Visible)
            {
                PlayBtnFullScreen_Click(sender, null);
            }
            else
            {

                PlayBtnExitFullScreen_Click(sender, null);
            }
        }
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            pointer_in_player = true;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            pointer_in_player = false;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.PointerCursor == null)
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            }

        }

        bool ManipulatingBrightness = false;
        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            //progress.Visibility = Visibility.Visible;
            if (ManipulatingBrightness)
                HandleSlideBrightnessDelta(e.Delta.Translation.Y);
            else
                HandleSlideVolumeDelta(e.Delta.Translation.Y);
        }


        private void HandleSlideVolumeDelta(double delta)
        {
            if (delta > 0)
            {
                double dd = delta / (this.ActualHeight * 0.8);

                //slider_V.Value -= d;
                var volume = mediaPlayer.Volume - dd;
                if (volume < 0) volume = 0;
                SliderVolume.Value = volume;

            }
            else
            {
                double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
                var volume = mediaPlayer.Volume + dd;
                if (volume > 1) volume = 1;
                SliderVolume.Value = volume;
                //slider_V.Value += d;
            }
            TxtToolTip.Text = "音量:" + mediaPlayer.Volume.ToString("P");

            //Utils.ShowMessageToast("音量:" +  mediaElement.MediaPlayer.Volume.ToString("P"), 3000);
        }
        private void HandleSlideBrightnessDelta(double delta)
        {
            double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
            if (delta > 0)
            {
                Brightness = Math.Min(Brightness + dd, 1);
            }
            else
            {
                Brightness = Math.Max(Brightness - dd, 0);
            }
            TxtToolTip.Text = "亮度:" + Math.Abs(Brightness - 1).ToString("P");
        }
        private void Grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
            TxtToolTip.Text = "";
            ToolTip.Visibility = Visibility.Visible;

            if (e.Position.X < this.ActualWidth / 2)
                ManipulatingBrightness = true;
            else
                ManipulatingBrightness = false;

        }

        double _brightness;
        double Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                BrightnessShield.Opacity = value;
                SettingHelper.SetValue<double>(SettingHelper.PLAYER_BRIGHTNESS, _brightness);
            }
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            ToolTip.Visibility = Visibility.Collapsed;
        }
        #endregion
        #region 窗口操作
        private void PlayBtnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            SetFullScreen(true);
        }

        private void PlayBtnExitFullScreen_Click(object sender, RoutedEventArgs e)
        {
            SetFullScreen(false);
        }

        private void PlayBtnExitFullWindow_Click(object sender, RoutedEventArgs e)
        {
            SetFullWindow(false);
        }

        private void PlayBtnFullWindow_Click(object sender, RoutedEventArgs e)
        {
            SetFullWindow(true);
        }

        private void PlayBtnMinWindow_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(true);
        }
        private void SetFullWindow(bool e)
        {

            if (e)
            {
                PlayBtnFullWindow.Visibility = Visibility.Collapsed;
                PlayBtnExitFullWindow.Visibility = Visibility.Visible;
                ColumnRight.Width = new GridLength(0, GridUnitType.Pixel);
                ColumnRight.MinWidth = 0;
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                PlayBtnFullWindow.Visibility = Visibility.Visible;
                PlayBtnExitFullWindow.Visibility = Visibility.Collapsed;
                ColumnRight.Width = new GridLength(SettingHelper.GetValue<double>(SettingHelper.RIGHT_DETAIL_WIDTH, 280), GridUnitType.Pixel);
                ColumnRight.MinWidth = 100;
                BottomInfo.Height = GridLength.Auto;
            }
        }
        private void SetFullScreen(bool e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            HideTitleBar(e);
            if (e)
            {

                PlayBtnFullScreen.Visibility = Visibility.Collapsed;
                PlayBtnExitFullScreen.Visibility = Visibility.Visible;

                ColumnRight.Width = new GridLength(0, GridUnitType.Pixel);
                ColumnRight.MinWidth = 0;

                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
                //全屏
                if (!view.IsFullScreenMode)
                {
                    view.TryEnterFullScreenMode();
                }
            }
            else
            {
                PlayBtnFullScreen.Visibility = Visibility.Visible;
                PlayBtnExitFullScreen.Visibility = Visibility.Collapsed;
                // 在恢复布局前，检查“铺满窗口”状态。
                if (PlayBtnFullWindow.Visibility == Visibility.Visible)
                {
                    var width = SettingHelper.GetValue<double>(SettingHelper.RIGHT_DETAIL_WIDTH, 280);
                    ColumnRight.Width = new GridLength(width, GridUnitType.Pixel);
                    //ColumnRight.Width = new GridLength(280, GridUnitType.Pixel);
                    ColumnRight.MinWidth = 100;
                    BottomInfo.Height = GridLength.Auto;
                }
                //退出全屏
                if (view.IsFullScreenMode)
                {
                    view.ExitFullScreenMode();
                }
            }
        }
        private async void MiniWidnows(bool mini)
        {
            HideTitleBar(mini);
            isMini = mini;
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (mini)
            {
                SetFullWindow(true);
                if (Utils.IsXbox && SettingHelper.GetValue<int>(SettingHelper.XBOX_MODE, 0) == 0)
                {
                    XBoxControl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    StandardControl.Visibility = Visibility.Collapsed;
                }

                MiniControl.Visibility = Visibility.Visible;

                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                {
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                    DanmuControl.DanmakuSizeZoom = 0.5;
                    DanmuControl.DanmakuDuration = 6;
                    DanmuControl.ClearAll();
                }
            }
            else
            {
                SetFullWindow(false);
                if (Utils.IsXbox && SettingHelper.GetValue<int>(SettingHelper.XBOX_MODE, 0) == 0)
                {
                    XBoxControl.Visibility = Visibility.Visible;
                }
                else
                {
                    StandardControl.Visibility = Visibility.Visible;
                }

                MiniControl.Visibility = Visibility.Collapsed;
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                DanmuControl.DanmakuSizeZoom = SettingHelper.GetValue<double>(SettingHelper.LiveDanmaku.FONT_ZOOM, 1);
                DanmuControl.DanmakuDuration = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.SPEED, 10);
                DanmuControl.ClearAll();
                DanmuControl.Visibility = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.SHOW, true) ? Visibility.Visible : Visibility.Collapsed;
            }

        }
        private void BottomBtnExitMiniWindows_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(false);
        }

        private async void PlayTopBtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            await CaptureVideo();
        }

        private void PlayBtnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void PlayBtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }


        #endregion

        private void BottomBtnShare_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.detail == null)
            {
                return;
            }
            Utils.SetClipboard(liveRoomVM.detail.Url);
            Utils.ShowMessageToast("已复制链接到剪切板");
        }

        private async void BottomBtnOpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.detail == null)
            {
                return;
            }
            await Windows.System.Launcher.LaunchUriAsync(new Uri(liveRoomVM.detail.Url));
        }

        private void BottomBtnPlayUrl_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.CurrentLine == null)
            {
                return;
            }
            Utils.SetClipboard(liveRoomVM.CurrentLine.Url);
            Utils.ShowMessageToast("已复制链接到剪切板");
        }

        private void BottomBtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (liveRoomVM.Loading) return;
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
                mediaPlayer.Source = null;
            }
            if (interopMSS != null)
            {
                interopMSS.Dispose();
                interopMSS = null;
            }

            liveRoomVM?.Stop();
            liveRoomVM.LoadData(pageArgs.Site, liveRoomVM.RoomID);
        }

        private void XboxSuperChat_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SuperChatItem;
            ContentDialog dialog = new ContentDialog
            {
                Title = item.UserName,
                Content = new TextBlock
                {
                    Text = item.Message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 20
                },
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = false,
                PrimaryButtonText = "确定"
            };
            _ = dialog.ShowAsync();
        }
    }
}
