using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AllLive.Avalonia.Views;
using AllLive.Core.Models;

namespace AllLive.Avalonia.ViewModels
{
    public class RecomendVM : BaseViewModel
    {
        public RecomendVM()
        {
            List<RecomendItemVM> ls = new List<RecomendItemVM>();
            foreach (var item in App.Sites)
            {
                ls.Add(new RecomendItemVM(item));
            }
            Items = ls;
        }

        private List<RecomendItemVM> items;
        public List<RecomendItemVM> Items
        {
            get { return items; }
            set { items = value; DoPropertyChanged("Items"); }
        }


    }
    public class RecomendItemVM : BaseViewModel
    {
        public Site Site { get; set; }
        public RecomendItemVM(Site site)
        {
            this.Site = site;
            Items = new ObservableCollection<Core.Models.LiveRoomItem>();
            OnClickCommand = new RelayCommand<object>(OnClick);
        }
        public ICommand OnClickCommand { get; set; }


        public ObservableCollection<AllLive.Core.Models.LiveRoomItem> Items { get; set; }
        public async void LoadData()
        {
            try
            {
                Loading = true;
                CanLoadMore = false;
                IsEmpty = false;
                var result = await Site.LiveSite.GetRecommendRooms(Page);
                foreach (var item in result.Rooms)
                {
                    Items.Add(item);
                }
                IsEmpty = Items.Count == 0;
                CanLoadMore = result.HasMore;
                Page++;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                Loading = false;
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            Items.Clear();
            LoadData();
        }

        public override void LoadMore()
        {
            base.LoadMore();
            LoadData();
        }

        public void OnClick(object data)
        {
            LiveRoomWindow window = new LiveRoomWindow(Site,data as LiveRoomItem);
            window.Show();
        }

    }
}
