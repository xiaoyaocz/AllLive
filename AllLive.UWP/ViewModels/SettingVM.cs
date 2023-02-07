using AllLive.UWP.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.ViewModels
{
    public class SettingVM
    {
        public SettingVM()
        {
            LoadShieldSetting();
        }
        public ObservableCollection<string> ShieldWords { get; set; }

        public void LoadShieldSetting()
        {
            ShieldWords =JsonConvert.DeserializeObject<ObservableCollection<string>>( SettingHelper.GetValue<string>(SettingHelper.LiveDanmaku.SHIELD_WORD,"[]"));
        }
    }
}
