using AllLive.Core.Interface;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace AllLive.Avalonia
{
    public class App : Application
    {
        public static List<Site> Sites = new List<Site>() {
           new Site("ﬂŸ¡®ﬂŸ¡®÷±≤•","/Assets/bilibili.png",new AllLive.Core.BiliBili()),
           new Site("∂∑”„÷±≤•","/Assets/douyu.png",new AllLive.Core.Douyu()),
           new Site("ª¢—¿÷±≤•","/Assets/huya.png",new AllLive.Core.Huya()),
        };
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }

    public class Site
    {
        public Site(string name, string logo, ILiveSite liveSite)
        {
            Name = name;
            Logo = logo;
            LiveSite = liveSite;
        }
        public string Name { get; set; }
        public ILiveSite LiveSite { get; set; }
        public string Logo { get; set; }
    }

}

