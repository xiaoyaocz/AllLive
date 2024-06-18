using AllLive.UWP.Helper;
using AllLive.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace AllLive.UWP.ViewModels
{
    public class FavoriteVM : BaseViewModel
    {
        public FavoriteVM()
        {
            Items = new ObservableCollection<FavoriteItem>();
        }

        private ObservableCollection<FavoriteItem> _items;
        public ObservableCollection<FavoriteItem> Items
        {
            get { return _items; }
            set { _items = value; DoPropertyChanged("Items"); }
        }


        private bool _loadingLiveStatus;

        public bool LoaddingLiveStatus
        {
            get { return _loadingLiveStatus; }
            set { _loadingLiveStatus = value; DoPropertyChanged("LoaddingLiveStatus"); }
        }



        public async void LoadData()
        {
            try
            {
                Loading = true;
                foreach (var item in await DatabaseHelper.GetFavorites())
                {
                    Items.Add(item);
                }
                IsEmpty = Items.Count == 0;
                LoadLiveStatus();
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

        public void LoadLiveStatus()
        {
            LoaddingLiveStatus = true;
            loadedCount = 0;
            foreach (var item in Items)
            {
                LoadLiveStatus(item);
            }
        }

        int loadedCount = 0;
        public async void LoadLiveStatus(FavoriteItem item)
        {
            try
            {
                var site = MainVM.Sites.FirstOrDefault(x => x.Name == item.SiteName);
                if (site != null)
                {
                    var status = await site.LiveSite.GetLiveStatus(item.RoomID);
                    item.LiveStatus = status;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"获取直播状态失败:{item.SiteName}-{item.RoomID}", LogType.ERROR, ex);
            }
            finally
            {
                loadedCount++;
                if (loadedCount == Items.Count)
                {
                    LoaddingLiveStatus = false;
                    loadedCount = 0;
                    // 排序，直播的在前面
                    Items = new ObservableCollection<FavoriteItem>(Items.OrderByDescending(x => x.LiveStatus));
                }
            }
        } 


        public override void Refresh()
        {
            base.Refresh();
            Items.Clear();
            LoadData();
        }

        public void RemoveItem(FavoriteItem item)
        {
            try
            {
                DatabaseHelper.DeleteFavorite(item.ID);
                Items.Remove(item);
                IsEmpty = Items.Count == 0;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

        }


    }
}
