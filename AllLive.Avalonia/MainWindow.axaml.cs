using AllLive.Avalonia.Views;
using AllLive.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using QuickJS;
using System.Collections.Generic;
using System.Linq;

namespace AllLive.Avalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
#if DEBUG
            this.AttachDevTools();
#endif
        }
        List<object> Pages = new List<object>() { 
            new RecommendView(),
            new TextBlock()
            {
                Text="分类"
            },
            new TextBlock()
            {
                Text="搜索"
            },
            new TextBlock()
            {
                Text="收藏"
            },
            new TextBlock()
            {
                Text="历史"
            },
            new TextBlock()
            {
                Text="设置"
            },
        };
        ListBox listMenu;
        ContentControl  content;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            listMenu = this.FindControl<ListBox>("listMenu");
            listMenu.SelectionChanged += ListMenu_SelectionChanged;
            content = this.FindControl<ContentControl>("content");

            content.Content = Pages[0];
        }

        private void ListMenu_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            content.Content = Pages[listMenu.SelectedIndex];
        }
    }
}
