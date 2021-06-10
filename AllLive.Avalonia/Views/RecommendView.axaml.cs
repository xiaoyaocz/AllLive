using AllLive.Avalonia.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

namespace AllLive.Avalonia.Views
{
    public partial class RecommendView : UserControl
    {
        readonly RecomendVM  recommendVM;
        public RecommendView()
        {
            recommendVM =new  RecomendVM();
            this.LayoutUpdated += RecommendView_LayoutUpdated;
            InitializeComponent();
        }

        double w = 0;
        private void RecommendView_LayoutUpdated(object sender, System.EventArgs e)
        {
            var v = this.Bounds.Width;
            if (w == v)
            {
                return;
            }
            w = v;

            var max = (int)(v / 360);
            if (max < 1) max = 1;
            var width = (v - (max * 16)) / max;
            itemWidth.Width = width;
            Debug.WriteLine(width);
        }

        TabControl tabControl;
        Border itemWidth;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            tabControl = this.FindControl<TabControl>("tabControl");
            itemWidth = this.FindControl<Border>("itemWidth");
            tabControl.SelectionChanged += TabControl_SelectionChanged;
            tabControl.Items = recommendVM.Items;
            RecommendView_LayoutUpdated(this,EventArgs.Empty);
        }

        private void TabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem == null) return;
            var vm = tabControl.SelectedItem as RecomendItemVM;
            if (vm.Loading == false && vm.Items.Count == 0)
            {
                vm.LoadData();
            }
        }
    }

}
