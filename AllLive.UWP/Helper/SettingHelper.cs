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
        public static LocalObjectStorageHelper storageHelper = new LocalObjectStorageHelper();
        public static T GetValue<T>(string key, T _default)
        {
            if (storageHelper.KeyExists(key))
            {
                return storageHelper.Read<T>(key);
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
        /// 鼠标功能键返回、关闭页面
        /// </summary>
        public const string MOUSE_BACK = "MouseBack";

        /// <summary>
        /// 软解
        /// </summary>
        public const string SORTWARE_DECODING = "sortwareDecoding";

        /// <summary>
        /// 音量
        /// </summary>
        public const string PLAYER_VOLUME = "PlayerVolume";
        /// <summary>
        /// 亮度
        /// </summary>
        public const string PLAYER_BRIGHTNESS = "PlayeBrightness";
        public class LiveDanmaku
        {
            public const string TOP_MARGIN = "LiveTopMargin";
            /// <summary>
            /// 显示弹幕 Visibility
            /// </summary>
            public const string SHOW = "LiveDanmuShow";
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
            public const string SHIELD_WORD = "LiveDanmuShieldWord";

            /// <summary>
            /// 直播弹幕清理
            /// </summary>
            public const string DANMU_CLEAN_COUNT = "LiveCleanCount";
        }
    }
}
