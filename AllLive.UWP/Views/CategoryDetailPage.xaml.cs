using AllLive.Core.Models;
using AllLive.UWP.Helper;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AllLive.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CategoryDetailPage : Page
    {
        readonly CategoryDetailVM categoryDetailVM;
        PageArgs pageArgs;
        public CategoryDetailPage()
        {
            categoryDetailVM = new CategoryDetailVM();
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
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
            if (e.NavigationMode == NavigationMode.New)
            {
                pageArgs = e.Parameter as PageArgs;
                var category = pageArgs.Data as LiveSubCategory;
                MessageCenter.ChangeTitle(category.Name, pageArgs.Site);
                //txtTitle.Text = pageArgs.Site.Name+" - " +category.Name;
                categoryDetailVM.LoadData(pageArgs.Site, category);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                MessageCenter.ChangeTitle(categoryDetailVM.Category.Name, categoryDetailVM.Site);

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        private void MyAdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as AllLive.Core.Models.LiveRoomItem;
            MessageCenter.OpenLiveRoom(pageArgs.Site, item);

        }
    }
}
