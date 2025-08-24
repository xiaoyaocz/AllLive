using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.Helper
{
    public class SettingHelper
    {
        public static ApplicationDataStorageHelper storageHelper = ApplicationDataStorageHelper.GetCurrent(new Microsoft.Toolkit.Helpers.SystemSerializer());
        public static T GetValue<T>(string key, T _default)
        {
            if (storageHelper.KeyExists(key))
            {
                var value = storageHelper.Read<T>(key);
                return value==null?_default:value;
            }
            else
            {
                return _default;
            }
        }
        public static void SetValue<T>(string key, T value)
        {
            storageHelper.Save<T>(key, value);
        }
        /// <summary>
        /// 主题,0为默认，1为浅色，2为深色
        /// </summary>
        public const string THEME = "theme";
        /// <summary>
        /// 互动文字大小
        /// </summary>
        public const string MESSAGE_FONTSIZE = "MessageFontSize";
        /// <summary>
        /// 右侧详情宽度
        /// </summary>
        public const string RIGHT_DETAIL_WIDTH = "PlayerRightDetailWidth";

        /// <summary>
        /// 新窗口打开直播间
        /// </summary>
        public const string NEW_WINDOW_LIVEROOM = "newWindowLiveRoom";
        /// <summary>
        /// 直播默认铺满窗口
        /// </summary>
        public const string DEFAULT_FULL_WINDOW = "defaultFullWindow";
        /// <summary>
        /// 鼠标功能键返回、关闭页面
        /// </summary>
        public const string MOUSE_BACK = "MouseBack";

        /// <summary>
        /// 视频解码
        /// </summary>
        public const string VIDEO_DECODER = "VideoDecoder";

        /// <summary>
        /// 音量
        /// </summary>
        public const string PLAYER_VOLUME = "PlayerVolume";
        /// <summary>
        /// 亮度
        /// </summary>
        public const string PLAYER_BRIGHTNESS = "PlayeBrightness";

        /// <summary>
        /// 哔哩哔哩Cookie
        /// </summary>
        public const string BILI_COOKIE = "BiliCookie";

        /// <summary>
        /// 哔哩哔哩用户ID
        /// </summary>
        public const string BILI_USER_ID = "BiliUserId";

        /// <summary>
        /// NavigationView导航栏显示模式
        /// </summary>
        public const string PANE_DISPLAY_MODE = "PaneDisplayMode";

        /// <summary>
        /// 忽略哔哩哔哩登录提醒
        /// </summary>
        public const string IGNORE_BILI_LOGIN_TIP = "IgnoreBiliLoginTip";

        /// <summary>
        /// XBOX模式
        /// </summary>
        public const string XBOX_MODE = "XboxMode";

        public class LiveDanmaku
        {
            public const string TOP_MARGIN = "LiveTopMargin";
            /// <summary>
            /// 显示弹幕 
            /// </summary>
            public const string SHOW = "LiveDanmuShowBool";
            /// <summary>
            /// 弹幕显示区域
            /// </summary>
            public const string AREA = "LiveDanmuArea";
            /// <summary>
            /// 弹幕缩放 double
            /// </summary>
            public const string FONT_ZOOM = "LiveDanmuFontZoom";
            /// <summary>
            /// 弹幕速度 int
            /// </summary>
            public const string SPEED = "LiveDanmuSpeed";
            /// <summary>
            /// 弹幕加粗 bool
            /// </summary>
            public const string BOLD = "LiveDanmuBold";
            /// <summary>
            /// 彩色弹幕 bool
            /// </summary>
            public const string COLOURFUL = "LiveDanmuColourful";
            /// <summary>
            /// 弹幕边框样式 int
            /// </summary>
            public const string BORDER_STYLE = "LiveDanmuStyle";

            /// <summary>
            /// 弹幕透明度 double，0-1
            /// </summary>
            public const string OPACITY = "LiveDanmuOpacity";
            /// <summary>
            /// 关键词屏蔽 ObservableCollection<string>
            /// </summary>
            public const string SHIELD_WORD = "LiveDanmuShieldWordString1";

            /// <summary>
            /// 直播弹幕清理
            /// </summary>
            public const string DANMU_CLEAN_COUNT = "LiveCleanCount";

            /// <summary>
            /// 保留醒目留言
            /// </summary>
            public const string KEEP_SUPER_CHAT = "KeepSuperChat";
        }
    }
}
