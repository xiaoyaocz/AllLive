using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AllLive.UWP.ViewModels
{
    public class CategoryVM : BaseViewModel
    {

        public CategoryVM()
        {
            List<CategoryItemVM> ls = new List<CategoryItemVM>();
            foreach (var item in MainVM.Sites)
            {
                ls.Add(new CategoryItemVM(item));
            }
            Items = ls;
        }

        private List<CategoryItemVM> items;
        public List<CategoryItemVM> Items
        {
            get { return items; }
            set { items = value; DoPropertyChanged("Items"); }
        }
    }
    public class CategoryItemVM : BaseViewModel
    {
        public readonly Site site;
        public CategoryItemVM(Site site)
        {
            this.site = site;
           // Items = new ObservableCollection<Core.Models.LiveCategory>();
        }
        //public ObservableCollection<AllLive.Core.Models.LiveCategory> Items { get; set; }

        private CollectionViewSource collectionView;
        public CollectionViewSource CollectionView
        {
            get { return collectionView; }
            set { collectionView = value; DoPropertyChanged("CollectionView"); }
        }

        public async void LoadData()
        {
            try
            {
                Loading = true;
                var result = await site.LiveSite.GetCategores();
                CollectionViewSource collectionViewSource = new CollectionViewSource();
                collectionViewSource.IsSourceGrouped = true;
                collectionViewSource.ItemsPath = new PropertyPath("Children");
                collectionViewSource.Source = result;
                CollectionView = collectionViewSource;
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
            LoadData();
        }

       

    }
}
