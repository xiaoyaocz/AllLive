using AllLive.Core.Interface;
using AllLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.ViewModels
{
    public class CategoryDetailVM:BaseViewModel
    {
        public CategoryDetailVM()
        {
            Items = new ObservableCollection<Core.Models.LiveRoomItem>();
        }

        public ObservableCollection<AllLive.Core.Models.LiveRoomItem> Items { get; set; }

        ILiveSite _site;
        LiveSubCategory _category;
        public async void LoadData(ILiveSite site, LiveSubCategory category)
        {
            try
            {
                _site = site;
                _category = category;
                Loading = true;
                CanLoadMore = false;
                var result = await site.GetCategoryRooms(category, Page);
                foreach (var item in result.Rooms)
                {
                    Items.Add(item);
                }
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
            LoadData(_site, _category);
        }

        public override void LoadMore()
        {
            base.LoadMore();
            LoadData(_site, _category);
        }
    }
}
