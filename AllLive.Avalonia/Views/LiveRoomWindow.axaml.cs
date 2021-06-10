using AllLive.Core.Interface;
using AllLive.Core.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using System.Diagnostics;

namespace AllLive.Avalonia.Views
{
    public partial class LiveRoomWindow : Window
    {
        readonly Site site;
        readonly LiveRoomItem roomItem;
        private LibVLC _libVlc;
        private MediaPlayer MediaPlayer;
        public LiveRoomWindow()
        {
            this.InitializeComponent();


        }
        public LiveRoomWindow(Site site,LiveRoomItem item)
        {
            InitializeComponent();
            this.site = site;
            this.roomItem = item;
            _libVlc = new LibVLC();
            MediaPlayer = new MediaPlayer(_libVlc);
            this.FindControl<VideoView>("videoView").MediaPlayer = MediaPlayer;
            _libVlc.Log += _libVlc_Log;
           
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private void _libVlc_Log(object sender, LogEventArgs e)
        {
            Debug.WriteLine(e.FormattedLog);
        }
        //VideoView videoView;
        ListBox listChat;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
           // videoView = this.FindControl<VideoView>("videoView");
            //videoView.MediaPlayer = MediaPlayer;
            listChat = this.FindControl<ListBox>("listChat");
            listChat.Items = Chats;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            LoadData();
        }
        ILiveDanmaku danmaku;
        System.Collections.ObjectModel.ObservableCollection<string> Chats = new System.Collections.ObjectModel.ObservableCollection<string>();
        private async void LoadData()
        {
            var detail=await site.LiveSite.GetRoomDetail(roomItem.RoomID);
            this.Title = detail.Title;
            var q = await site.LiveSite.GetPlayQuality(detail);
            var url= await site.LiveSite.GetPlayUrls(detail,q[0]);

            SetPlayer(url[0]);
            danmaku = site.LiveSite.GetDanmaku();
            danmaku.NewMessage += Danmaku_NewMessage;
            await danmaku.Start(detail.DanmakuData);

        }

        private void Danmaku_NewMessage(object? sender, LiveMessage e)
        {
            Chats.Add($"{e.UserName}£∫{e.Message}");
            Dispatcher.UIThread.InvokeAsync(() => {
                listChat.Scroll.Offset = new Vector(0, listChat.Scroll.Extent.Height);
            });
        }

        private void SetPlayer(string video)
        {
            using var media = new Media(_libVlc, new Uri(video));
            if (site.Name == "ﬂŸ¡®ﬂŸ¡®÷±≤•")
            {
                media.AddOption("http-referrer=https://live.bilibili.com");
                media.AddOption("http-user-agent=Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)");
            }
          
            MediaPlayer.EnableHardwareDecoding = true;
            MediaPlayer.Play(media);
        }
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                if (danmaku!=null)
                {
                    danmaku.NewMessage -= Danmaku_NewMessage;
                    danmaku?.Stop();
                }
             
                MediaPlayer?.Stop();
                //MediaPlayer?.Dispose();
                MediaPlayer = null;
                _libVlc?.Dispose();
                _libVlc = null;
                GC.Collect();
            }
            catch (Exception)
            {
            }

            base.OnClosed(e);
        }

    }
}
