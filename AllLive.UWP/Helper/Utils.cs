using AllLive.UWP.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllLive.UWP.Helper
{
    public static class Utils
    {
        public  static void ShowMessageToast(string message, int seconds = 2)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds));
            ms.Show();
        }
    }
}
