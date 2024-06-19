using AllLive.Core;
using AllLive.Core.Helper;
using AllLive.UWP.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.Helper
{
    public class BiliAccount
    {
        public event EventHandler OnAccountChanged;

        private static BiliAccount instance;
        public static BiliAccount Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BiliAccount();
                }
                return instance;
            }
        }
        public bool Logined { get; set; } = false;
        public string UserName { get; set; } = "";
        public long UserId {
            get
            {
                return SettingHelper.GetValue<long>(SettingHelper.BILI_USER_ID, 0L);
            }
        } 
        public string Cookie
        {
            get
            {
                return SettingHelper.GetValue<string>(SettingHelper.BILI_COOKIE, "");
            }
        }

        public async Task InitLoginInfo()
        {

            Logined = !string.IsNullOrEmpty(Cookie);
            if (Logined)
            {
                SetBiliSiteCookie();
                await LoadUserInfo();
            }
           

        }

        public async Task LoadUserInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(Cookie))
                {
                    return;
                }
                var resp = await HttpUtil.GetString("https://api.bilibili.com/x/member/web/account", headers: new Dictionary<string, string>
                {
                    { "cookie", Cookie }
                });
                var json = JObject.Parse(resp);
                var code = json["code"].ToObject<int>();
                if (code == 0)
                {
                    UserName = json["data"]["uname"].ToString();

                    SettingHelper.SetValue(SettingHelper.BILI_USER_ID, json["data"]["mid"].ToObject<long>());
                   
                    Logined = true;
                    SetBiliSiteCookie();
                    OnAccountChanged?.Invoke(this, null);
                }
                else
                {
                    Utils.ShowMessageToast("哔哩哔哩登录已失效，请重新登录");
                    Logout();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("获取哔哩哔哩用户信息失败", LogType.ERROR, ex);
                Utils.ShowMessageToast("获取哔哩哔哩用户信息失败，可前往设置中重试");
            }
        }


        public void SetBiliSiteCookie()
        {
            var site = MainVM.Sites.FirstOrDefault(x => x.SiteType == LiveSite.Bilibili);
            (site.LiveSite as BiliBili).Cookie = Cookie;
            (site.LiveSite as BiliBili).UserId = UserId;
        }

        public void Logout()
        {
            Logined = false;
            SettingHelper.SetValue(SettingHelper.BILI_COOKIE, "");
            SettingHelper.SetValue(SettingHelper.BILI_USER_ID, 0L);
            UserName = "";
           
            SetBiliSiteCookie();
            OnAccountChanged?.Invoke(this, null);
        }

    }
}
