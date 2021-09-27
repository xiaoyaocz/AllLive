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
    public sealed partial class CategoryPage : Page
    {
        readonly CategoryVM categoryVM;
        public CategoryPage()
        {
            categoryVM = new CategoryVM();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.InitializeComponent();
        }
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == null) return;
            var vm = pivot.SelectedItem as CategoryItemVM;
            if (vm.Loading == false && vm.CollectionView == null)
            {
                vm.LoadData();
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as LiveSubCategory;
            MessageCenter.NavigatePage(typeof(CategoryDetailPage), new PageArgs()
            {
                Site = (pivot.SelectedItem as CategoryItemVM).site.LiveSite,
                Data = item
            });
            //(Window.Current.Content as Frame).Navigate(typeof(CategoryDetailPage), new PageArgs()
            //{
            //    Site = (pivot.SelectedItem as CategoryItemVM).site.LiveSite,
            //    Data = item
            //});
        }
    }
}
