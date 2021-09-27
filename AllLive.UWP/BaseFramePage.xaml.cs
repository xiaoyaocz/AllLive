using AllLive.UWP.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AllLive.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BaseFramePage : Page
    {
        public BaseFramePage()
        {
            this.InitializeComponent();
            MessageCenter.NavigatePageEvent += MessageCenter_NavigatePageEvent;
            MessageCenter.ChangeTitleEvent += MessageCenter_ChangeTitleEvent;
            MessageCenter.HideTitlebarEvent += MessageCenter_HideTitlebarEvent;
            this.PointerPressed += BaseFramePage_PointerPressed;
            BtnBack.Click += BtnBack_Click;
            MainFrame.Navigated += MainFrame_Navigated;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += BaseFramePage_BackRequested;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(TitleBar);
        }

        private void MessageCenter_HideTitlebarEvent(object sender, bool e)
        {
            try
            {
                if (e)
                {
                    Grid.SetRow(MainFrame, 0);
                    Grid.SetRowSpan(MainFrame, 2);
                    TitleBarGrid.Visibility = Visibility.Collapsed;
                    TitleBar2.Visibility = Visibility.Visible;
                    Window.Current.SetTitleBar(TitleBar2);
                }
                else
                {
                    Grid.SetRow(MainFrame, 1);
                    Grid.SetRowSpan(MainFrame, 1);
                    TitleBarGrid.Visibility = Visibility.Visible;
                    TitleBar2.Visibility = Visibility.Collapsed;
                    Window.Current.SetTitleBar(TitleBar);
                }
            }
            catch (Exception)
            {

                
            }
           
        }

        private void MessageCenter_ChangeTitleEvent(string title, string logo)
        {
            try
            {
                Title.Text = title;
                AppIcon.Source = new BitmapImage(new Uri(logo));
            }
            catch (Exception)
            {
                //TODO 新窗口调用此方法会出现线程错误，待处理
                //throw;
            }
           
           
        }

        private void MessageCenter_NavigatePageEvent(Type page, object data)
        {
            try
            {
                MainFrame.Navigate(page, data);
            }
            catch (Exception)
            {

               
            }
            
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Title.Text = "聚合直播";
            AppIcon.Source = new BitmapImage(new Uri("ms-appx:///Assets/Square44x44Logo.png"));
            BtnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BaseFramePage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var par = e.GetCurrentPoint(sender as Page).Properties.PointerUpdateKind;
            if (SettingHelper.GetValue<bool>(SettingHelper.MOUSE_BACK, true) && par == Windows.UI.Input.PointerUpdateKind.XButton1Pressed || par == Windows.UI.Input.PointerUpdateKind.MiddleButtonPressed)
            {
                if (MainFrame.CanGoBack)
                {
                    MainFrame.GoBack();
                    e.Handled = true;
                }

            }
        }

        private void BaseFramePage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MainFrame.Navigate(typeof(MainPage));
        }


    }
}
