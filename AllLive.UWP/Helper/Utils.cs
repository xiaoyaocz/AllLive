using AllLive.Core.Helper;
using AllLive.UWP.Controls;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace AllLive.UWP.Helper
{
    public static class Utils
    {
        public  static void ShowMessageToast(string message, int seconds = 2)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds));
            ms.Show();
        }
        public async static Task<bool> ShowDialog(string title, string content)
        {
            MessageDialog messageDialog = new MessageDialog(content, title);
            messageDialog.Commands.Add(new UICommand() { Label = "确定", Id = true });
            messageDialog.Commands.Add(new UICommand() { Label = "取消", Id = false });
            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }
        public static bool SetClipboard(string content)
        {
            try
            {
                Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
                pack.SetText(content);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack);
                Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public static Task AnimateDoublePropertyAsync(this DependencyObject target, string property, double from, double to, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Storyboard storyboard = AnimateDoubleProperty(target, property, from, to, duration, easingFunction);
            storyboard.Completed += (sender, e) =>
            {
                tcs.SetResult(true);
            };
            return tcs.Task;
        }
        public static Storyboard AnimateDoubleProperty(this DependencyObject target, string property, double from, double to, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = easingFunction ?? new SineEase(),
                FillBehavior = FillBehavior.HoldEnd,
                EnableDependentAnimation = true
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, property);

            storyboard.Children.Add(animation);
            storyboard.FillBehavior = FillBehavior.HoldEnd;
            storyboard.Begin();

            return storyboard;
        }
        public static async Task FadeInAsync(this UIElement element, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            if (element.Opacity < 1.0)
            {
                await AnimateDoublePropertyAsync(element, "Opacity", element.Opacity, 1.0, duration, easingFunction);
            }
        }

     
        public static async Task FadeOutAsync(this UIElement element, double duration = 250, EasingFunctionBase easingFunction = null)
        {
            if (element.Opacity > 0.0)
            {
                await AnimateDoublePropertyAsync(element, "Opacity", element.Opacity, 0.0, duration, easingFunction);
            }
        }

        public async static Task CheckVersion()
        {
            try
            {
                var url = $"https://cdn.jsdelivr.net/gh/xiaoyaocz/AllLive@master/AllLive.UWP/version.json?ts{new Random().Next(0,99999) }";
                var result = await HttpUtil.GetString(url);
                var ver = JsonConvert.DeserializeObject<NewVersion>(result);
                var num = $"{ SystemInformation.Instance.ApplicationVersion.Major }{ SystemInformation.Instance.ApplicationVersion.Minor.ToString("00")}{ SystemInformation.Instance.ApplicationVersion.Build.ToString("00")}";
                var v = int.Parse(num);
                if (ver.versionCode > v)
                {
                    var dialog = new ContentDialog();
                    dialog.Title = $"发现新版本 Ver {ver.version}";
                    TextBlock markdownText = new TextBlock()
                    {
                        Text = ver.message,
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                    };
                    dialog.Content = markdownText;
                    dialog.PrimaryButtonText = "查看详情";
                    dialog.SecondaryButtonText = "忽略";
                    dialog.PrimaryButtonClick += new Windows.Foundation.TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(async (sender, e) =>
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(ver.url));
                    });
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
            }
        }

    }
    public class NewVersion
    {
        public string version { get; set; }
        public int versionCode { get; set; }
        public string message { get; set; }
        public string url { get; set; }
    }
}
