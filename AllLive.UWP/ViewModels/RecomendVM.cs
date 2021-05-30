using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AllLive.UWP.Helper;

namespace AllLive.UWP.ViewModels
{
    public class RecomendVM : BaseViewModel
    {
        public RecomendVM()
        {
            List<RecomendItemVM> ls = new List<RecomendItemVM>();
            foreach (var item in MainVM.Sites)
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
        public readonly Site site;
        public RecomendItemVM(Site site)
        {
            this.site = site;
            Items = new ObservableCollection<Core.Models.LiveRoomItem>();
        }
        public ObservableCollection<AllLive.Core.Models.LiveRoomItem> Items { get; set; }
        public async void LoadData()
        {
            try
            {
                Loading = true;
                CanLoadMore = false;
                IsEmpty = false;
                var result = await site.LiveSite.GetRecommendRooms(Page);
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
    }
}
