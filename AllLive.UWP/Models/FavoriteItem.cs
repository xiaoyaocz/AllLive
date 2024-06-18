using AllLive.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.Models
{
    public class FavoriteItem: BaseNotifyPropertyChanged
    {
        public int ID { get; set; }
        public string RoomID { get; set; }
        public string UserName { get; set; }
        public string Photo { get; set; }
        public string SiteName { get; set; }


        private bool _LiveStatus=false;
        public bool LiveStatus
        {
            get { return _LiveStatus; }
            set { _LiveStatus = value; DoPropertyChanged("LiveStatus"); }
        }

    }
}
