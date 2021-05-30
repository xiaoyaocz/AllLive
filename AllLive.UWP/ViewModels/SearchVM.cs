using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.ViewModels
{
    public  class SearchVM:BaseViewModel
    {
        public SearchVM()
        {
            List<SearchItemVM> ls = new List<SearchItemVM>();
            foreach (var item in MainVM.Sites)
            {
                ls.Add(new SearchItemVM(item));
            }
            Items = ls;
        }

        private List<SearchItemVM> items;
        public List<SearchItemVM> Items
        {
            get { return items; }
            set { items = value; DoPropertyChanged("Items"); }
        }
     
    }
    public class SearchItemVM : BaseViewModel
    {
        public readonly Site site;
        public SearchItemVM(Site site)
        {
            this.site = site;
             Items = new ObservableCollection<Core.Models.LiveRoomItem>();
        }
        public ObservableCollection<AllLive.Core.Models.LiveRoomItem> Items { get; set; }
        private string _keyword="";
        public async void LoadData(string keyword)
        {
            try
            {
                _keyword = keyword;
                Loading = true;
                CanLoadMore = false;
                IsEmpty = false;
                var result = await site.LiveSite.Search(keyword);
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
            LoadData(_keyword);
        }

        public override void LoadMore()
        {
            base.LoadMore();
            LoadData(_keyword);
        }
    }
}
